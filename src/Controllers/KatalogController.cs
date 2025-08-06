using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AasDemoapp.Database.Model;
using AasDemoapp.Database.Model.DTOs;
using AasDemoapp.Katalog;
using Microsoft.AspNetCore.Mvc;

namespace AasDemoapp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class KatalogController : ControllerBase
    {
        private readonly KatalogService _katalogService;
        private readonly IMapper _mapper;

        public KatalogController(KatalogService katalogService, IMapper mapper)
        {
            _katalogService = katalogService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<List<KatalogEintragDto>> GetAllRohteil()
        {
            var katalogEintraege = await Task.FromResult(_katalogService.GetAll(KatalogEintragTyp.RohteilTyp));
            return _mapper.Map<List<KatalogEintragDto>>(katalogEintraege);
        }

        [HttpGet]
        public async Task<List<KatalogEintragDto>> GetAllRohteilInstanz()
        {
            var katalogEintraege = await Task.FromResult(_katalogService.GetAll(KatalogEintragTyp.RohteilInstanz));
            return _mapper.Map<List<KatalogEintragDto>>(katalogEintraege);
        }

        public class RndResult
        {
            public string id { get; set; } = string.Empty;
        }

        [HttpGet]
        public async Task<RndResult> GetRandomRohteil()
        {
            // wären die instanzen
            List<string> eintraege = ["https://meta-level.de/ids/asset/5608_3028_6333_7157", "https://meta-level.de/ids/asset/4094_2880_5477_5296", "https://meta-level.de/ids/asset/4633_9370_4324_9117", "https://meta-level.de/ids/asset/6080_8024_1964_2138", "https://meta-level.de/ids/asset/8033_4071_2840_3443", "https://meta-level.de/ids/asset/1039_7696_3939_2957", "https://meta-level.de/ids/asset/9745_9188_5962_5247", "https://meta-level.de/ids/asset/3231_2344_2659_9352"];
            var random = new Random();
            var res = new RndResult()
            {
                id = eintraege[random.Next(eintraege.Count)]
            };
            return await Task.FromResult(res);

            // var eintraege = _katalogService.GetAll(KatalogEintragTyp.RohteilTyp);
            // if (eintraege.Count == 0)
            // {
            //     return new RndResult { id = string.Empty };
            // }
            // else
            // {
            //     var random = new Random();
            //     var res = new RndResult()
            //     {
            //         id = eintraege[random.Next(eintraege.Count)].GlobalAssetId
            //     };
            //     return await Task.FromResult(res);
            // }
        }

        [HttpPost]
        public async Task<KatalogEintrag> ImportRohteilTyp([FromBody] KatalogEintrag katalogEintrag)
        {
            return await _katalogService.ImportRohteilTyp(katalogEintrag);
        }

        [HttpGet]
        public async Task<RohteilLookupResult?> LookupRohteil(string instanzGlobalAssetId)
        {
            return await _katalogService.LookupRohteil(instanzGlobalAssetId);
        }

        [HttpDelete]
        public async Task Delete(long id)
        {
            await _katalogService.Delete(id);
        }

        [HttpPost]
        public async Task<KatalogEintrag?> ImportRohteilInstanz(KatalogEintrag katalogEintrag)
        {
            return await _katalogService.ImportRohteilInstanz(katalogEintrag);
        }

        [HttpGet]
        public async Task<KatalogEintrag?> GetRohteilInstanz(string globalAssetId)
        {
            return await Task.FromResult(_katalogService.GetRohteilKatalogEintrag(globalAssetId));
        }
    }
}