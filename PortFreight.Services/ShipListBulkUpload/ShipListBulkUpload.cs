using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using PortFreight.Data;
using PortFreight.Services.Common;
using PortFreight.Services.Interface;
using PortFreight.Services.FleUpload;

namespace PortFreight.Services.Import
{
    public class ShipListBulkUpload : IShipListBulkUpload 
    {
        private readonly PortFreightContext _context;
        private IFileUploadValidator _fileUploadValidator;
        private const string WorldFleetTableName = "world_fleet";
        private const string WorldFleetArchiveTableName = "world_fleet_archive";
        private const string WorldFleetTemporaryUploadTableName = "world_fleet_temp_upload";
        private int NumberOfRecordsInUploadedFile { get; set; }

        public ShipListBulkUpload(PortFreightContext context,
                                 IFileUploadValidator fileUploadValidator)            
        {
            _context = context;
            _fileUploadValidator = fileUploadValidator;           
        }
        
        /// <param name="uploadedShipList"></param>
        /// <param name="uploadedByUserName"></param>
        /// <returns> A method result object</returns>
        public MethodResult BulkUploadShipList(IFormFile uploadedShipList, string uploadedByUserName)
        {
            var returnValue = UploadShipList(uploadedShipList, uploadedByUserName);
            var methodResult = new MethodResult();
            var fileName = uploadedShipList.FileName;

            switch (returnValue)
            {
                case Enums.ShipListUploadOutcomes.SavedSuccessfully:
                    methodResult.SuccessFaliure = Enums.MethodResultOutcome.Success;
                    methodResult.Message = @"File """ + fileName + @"""  has been uploaded successfully";
                    break;
                case Enums.ShipListUploadOutcomes.FileTypeNotCsv:
                    methodResult.Message = @"File """ + fileName + @""" is not a valid csv file.";
                    break;
                case Enums.ShipListUploadOutcomes.EmptyFile:
                    methodResult.Message = @"File """ + fileName + @"""  is empty. Please upload a valid file";
                    break;
                case Enums.ShipListUploadOutcomes.IncorrectHeader:
                    methodResult.Message = @"File """ + fileName + @"""  has incorrect header. Please upload in the correct format.";
                    break;
                case Enums.ShipListUploadOutcomes.ErrorWhileProcessingFile:
                    methodResult.Message = "Error occurred while uploading file " + fileName + " . Please retry.";
                    break;
                case Enums.ShipListUploadOutcomes.PotentialMissingRecords:
                    methodResult.Message =  @"File """ + fileName + @""" has data errors or incorrect data format, the file upload is unsuccessful. Please fix the errors in file and retry.";
                    break;
            }
            return methodResult;           
        }

        /// <param name="uploadedShipList"></param>
        /// <param name="uploadedByUserName"></param>
        private Enums.ShipListUploadOutcomes UploadShipList(IFormFile uploadedShipList, string uploadedByUserName)
        {
            var _uploadedFile = CreateUploadFileObject(uploadedShipList);
                      
            if (!_fileUploadValidator.FileContentTypeCSV(_uploadedFile.contentType)) return Enums.ShipListUploadOutcomes.FileTypeNotCsv;

            if (!_fileUploadValidator.ValidFileHeader(_uploadedFile.headerRowData, CommonConstants.SHIPLISTFILEHEADER)) return Enums.ShipListUploadOutcomes.IncorrectHeader;

            return _uploadedFile.NumberOfDataRows == 0 ? Enums.ShipListUploadOutcomes.EmptyFile : UploadFile(uploadedShipList, _uploadedFile);
        }

        private Enums.ShipListUploadOutcomes UploadFile(IFormFile uploadedShipList, UploadedFile _uploadedFile)
        {
            var methodReturnValue = Enums.ShipListUploadOutcomes.ErrorWhileProcessingFile;

            using (var conn = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
            {            
                try
                {
                    conn.Open();
                    if (!BulkUploadToTemporaryTableSuccessful(uploadedShipList, conn))
                    {
                        return Enums.ShipListUploadOutcomes.PotentialMissingRecords;
                    }
                }
                catch ( Exception )
                {
                    conn.Close();
                    return Enums.ShipListUploadOutcomes.ErrorWhileProcessingFile;
                }

                using (var transaction = conn.BeginTransaction())
                {
                    var mySqlCommand = conn.CreateCommand();
                    mySqlCommand.Transaction = transaction;
                    try
                    {
                        DeleteTableRows(mySqlCommand, WorldFleetArchiveTableName);
                        mySqlCommand.CommandText = "INSERT INTO " + WorldFleetArchiveTableName + " SELECT * FROM " + WorldFleetTableName;
                        mySqlCommand.ExecuteNonQuery();

                        DeleteTableRows(mySqlCommand, WorldFleetTableName);
                        mySqlCommand.CommandText = "INSERT INTO " + WorldFleetTableName + " SELECT * FROM " + WorldFleetTemporaryUploadTableName ;
                        mySqlCommand.ExecuteNonQuery();

                        transaction.Commit();
                        methodReturnValue = Enums.ShipListUploadOutcomes.SavedSuccessfully;
                    }
                    catch (MySqlException e)
                    {
                        if (e.Number != 1062) return methodReturnValue;
                        transaction.Rollback();
                        methodReturnValue = Enums.ShipListUploadOutcomes.PotentialMissingRecords;
                    }
                    catch (Exception)
                    {
                        if (conn.State == System.Data.ConnectionState.Open)
                        {
                            transaction.Rollback();
                        }                        
                    }
                }
            }
            return methodReturnValue;
        }
      

        private  void DeleteTableRows(MySqlCommand mySqlCommand, string tableName)
        {         
            mySqlCommand.CommandText = "DELETE FROM " + tableName;
            mySqlCommand.ExecuteNonQuery();
        }

        /// <param name="uploadedShipList"></param>
        /// <param name="conn"></param>
        private bool BulkUploadToTemporaryTableSuccessful(IFormFile uploadedShipList, MySqlConnection conn)
        {
            CreateTemporaryTable(conn);
            var countOfRecordsUploaded = BulkUploadRecordCount(uploadedShipList, conn);
            if (countOfRecordsUploaded != NumberOfRecordsInUploadedFile)
                return false;

            return DataValidInUploadedFile(conn);     
        }

        private int BulkUploadRecordCount(IFormFile uploadedShipList, MySqlConnection conn)
        {
            var msbl = new MySqlBulkLoader(conn)
            {
                TableName = WorldFleetTemporaryUploadTableName,
                SourceStream = uploadedShipList.OpenReadStream(),
                FieldTerminator = ",",
                FieldQuotationCharacter = '"',
                NumberOfLinesToSkip = 1
            };
           return msbl.Load();            
        }

        private void CreateTemporaryTable(MySqlConnection conn)
        {
            using (var mySqlCommand = conn.CreateCommand())
            {
                mySqlCommand.CommandText = "CREATE TEMPORARY TABLE " + WorldFleetTemporaryUploadTableName + " SELECT * FROM " + WorldFleetTableName + " WHERE 1 =2";
                mySqlCommand.ExecuteNonQuery();
            }
        }

        /// <param name="conn"></param>
        private bool DataValidInUploadedFile(MySqlConnection conn)
        {
            using (var mySqlCommand = conn.CreateCommand())
            {
                mySqlCommand.CommandText = "SELECT COUNT(*) FROM " + WorldFleetTemporaryUploadTableName
                                        + " WHERE CHAR_LENGTH(imo) <> 7";
                var invalidRecords = (Int64)mySqlCommand.ExecuteScalar();
                return invalidRecords == 0;
            }
        }

        /// <param name="uploadedShipList"></param>
        private UploadedFile CreateUploadFileObject(IFormFile uploadedShipList)
        {
            UploadedFile uploadedFile = new UploadedFile();

            using (StreamReader streamReader = new StreamReader(uploadedShipList.OpenReadStream(), Encoding.UTF8))
            {
                uploadedFile.contentType = uploadedShipList.ContentType;
                uploadedFile.fileName = uploadedShipList.FileName;
                uploadedFile.headerRowData = streamReader.ReadLine();

                string rowData = null;

                for ( rowData = streamReader.ReadLine(); rowData != null; )
                {
                    uploadedFile.dataRows.Add(rowData);
                    rowData = streamReader.ReadLine();
                }
            }
            NumberOfRecordsInUploadedFile = uploadedFile.NumberOfDataRows;
            return uploadedFile;

        }
    }
}
