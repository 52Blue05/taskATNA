using Sale_Saas.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Common.Models;

public class LookupDto
{
    public int Id { get; init; }

    public string? TextSearch { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
           // CreateMap<GioiTinh, LookupDto>();
        }
    }
}
