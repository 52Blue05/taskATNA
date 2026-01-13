using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Features.UnitFeature.Dto;

public class CheckUnitPrerequisiteRequest
{
    public Guid? UnitId { get; set; }
    public Guid? SyllabusId { get; set; }
}
