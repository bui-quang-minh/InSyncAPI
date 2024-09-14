using AutoMapper;
using FakeItEasy;
using Repositorys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InSyncUnitTest.Controller
{
    public class TermsControllerTests
    {
        private readonly ITermRepository _termRepo;
        private readonly IMapper _mapper;
        public TermsControllerTests()
        {
            _termRepo = A.Fake<ITermRepository>();
            _mapper = A.Fake<Mapper>();
        }
       
    }
}
