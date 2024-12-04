using AutoMapper;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserProfileService(IUnitOfWork unitOfWork, IMapper mapper)
        {
           _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public Task CreateAsync()
        {
            throw new NotImplementedException();
        }
    }
}
