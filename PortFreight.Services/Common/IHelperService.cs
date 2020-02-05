using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using PortFreight.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PortFreight.Services.Common
{
    public interface IHelperService
    {
        byte GetCategoryCodeByDescription(string category);
        string GetCategoryDescriptionByCode(byte code);
        string GetPortCodeByPort(string portName);
        string GetCountryCodeByPort(string portName);
        string GetPortNameByCode(string code);
        string GenerateRandomAlphaNumericString();
        string GetUniqueKey();
        string GetShipNameByIMO(uint IMO);
        uint GetDeadweightByIMO(uint IMO);
        string GetCompanyNameBySenderID(string senderID);
        SenderType GetSenderType(string senderId);
        List<SelectListItem> GetPortsByCountryCode(string countryCode);
        List<string> GetRespondentPorts();
        List<string> GetReportingPorts();
        List<string> GetPortsOfLoadUnload();
        bool IsValidImo(uint imo);
        string GetFlagCodeFromIMO(uint imo);
        bool IsValidPort(string port);
        bool IsValidMsd2Port(string port);
        bool IsValidMsd3Port(string port);
        bool IsValidReportingPort(string port);
    }
}
