using System.Linq;
using PortFreight.Data;
using PortFreight.ViewModel;
using PortFreight.Services.Interface;
using System.Collections.Generic;

namespace PortFreight.Services.OrganisationList
{
    public class OrganisationSearch : IOrganisationSearch 
    {
        private PortFreightContext _context;
        private IEntityToViewModelMapper _entityToViewModelMapper;

        public OrganisationSearch(PortFreightContext context, IEntityToViewModelMapper entityToViewModelMapper)
        {
            _context = context;
            _entityToViewModelMapper = entityToViewModelMapper;
        }

        public List<OrganisationViewModel> Search(string searchInput)
        {
            var resultSet = _context.OrgList.AsQueryable();
            if (!string.IsNullOrEmpty(searchInput))
            {
                resultSet= resultSet.Where(x => x.OrgId.Replace(" ","").Contains(searchInput.Replace(" ", "")) || x.OrgName.Replace(" ", "").Contains(searchInput.Replace(" ","")));
            }            
           
            return _entityToViewModelMapper.MapToListOfOrganisationViewModel(resultSet.OrderBy(x => x.OrgName).Take(50).ToList());
        }

        public bool OrganisationNameDuplicate(OrganisationViewModel organisationVM)
        {
            var resultSet = _context.OrgList.AsQueryable();           
            resultSet = resultSet.Where(x => (x.OrgPkId != organisationVM.OrgPkId) && (x.OrgName.Replace(" ", "") == organisationVM.OrgName.Replace(" ", "")));
            return resultSet.Any();            
        }

        public bool OrganisationIdDuplicate(OrganisationViewModel organisationVM)
        {
            var resultSet = _context.OrgList.AsQueryable();
            resultSet = resultSet.Where(x => (x.OrgPkId != organisationVM.OrgPkId) && (x.OrgId.Replace(" ", "") == organisationVM.OrgId.Replace(" ", "")));
            return resultSet.Any();
        }
    }
}
