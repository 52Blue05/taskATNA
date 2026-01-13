using Sale_Saas.Application.Features.GainsFamilyFeature.Dto;
using Sale_Saas.Application.Features.GainsFeature.RequestModels;
using Sale_Saas.Application.Features.GainsSchoolFeature.Dto;

namespace Sale_Saas.Application.Features.GainsFeature.Services
{
    public class GainsService
    {
        public static async Task<bool> AddOrUpdateSchool(Guid id, List<GainsSchoolRequest> dataRequest, Guid user, IApplicationDbContext context, CancellationToken cancellationToken)
        {
            try
            {
                List<GainsSchoolDto> listSchool = new List<GainsSchoolDto>();

                foreach (GainsSchoolRequest request in dataRequest)
                {
                    if (request == null)
                    {
                        throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                    }

                    if (!string.IsNullOrEmpty(request.Name))
                    {
                        if (request.Year == null)
                        {
                            throw new ApplicationException($"Năm học không thể để trống");
                        }
                    }

                    var duplicateSchool = listSchool.Where(x => x.Name.ToLower().Trim() == request.Name.ToLower().Trim()
                                                             && x.Year == request.Year);

                    if (duplicateSchool.Any())
                        throw new ApplicationException($"Trường học không thể trùng nhau");

                    listSchool.Add(new GainsSchoolDto
                    {
                        Name = request.Name,
                        Year = request.Year,
                    });

                    if (request.Id == null)
                    {
                        var newGainsSchool = new GainsSchool
                        {
                            GainsId = id,
                            Name = request.Name,
                            Year = request.Year,
                            DeleteFlag = false,
                            CreatedApplicationUserId = user,
                            LastModifiedApplicationUserId = user,
                            CreatedDate = DateTime.Now,
                            LastModifiedDate = DateTime.Now
                        };

                        await context.GainsSchools.AddAsync(newGainsSchool);
                    }
                    else
                    {
                        var existingSchool = await context.GainsSchools
                             .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeleteFlag != true && x.GainsId == id, cancellationToken);

                        if (existingSchool == null)
                            throw new ApplicationException($"Không có dữ liệu Id School: {request.Id}.");

                        existingSchool.Name = request.Name;
                        existingSchool.Year = request.Year;
                        existingSchool.LastModifiedApplicationUserId = user;
                        existingSchool.LastModifiedDate = DateTime.Now;

                        context.GainsSchools.Update(existingSchool);
                    }
                }

                return true;
            }
            catch (Exception ex) { return false; }
        }

        public static async Task<bool> AddOrUpdateFamily(Guid id, List<GainsFamilyRequest> dataRequest, Guid user, IApplicationDbContext context, CancellationToken cancellationToken)
        {
            try
            {
                List<GainsFamilyDto> listFamily = new List<GainsFamilyDto>();

                foreach (GainsFamilyRequest request in dataRequest)
                {
                    if (request == null)
                    {
                        throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                    }
                    if (!string.IsNullOrEmpty(request.Name))
                    {
                        if (request.YearOfBirth == null)
                        {
                            throw new ApplicationException($"Năm sinh không thể để trống");
                        }
                    }

                    var duplicateFamily = listFamily.Where(x =>
                    x.Name.ToLower().Trim() == request.Name.ToLower().Trim() &&
                    x.YearOfBirth == request.YearOfBirth);

                    if (duplicateFamily.Any())
                        throw new ApplicationException($"Family không thể trùng nhau");

                    listFamily.Add(new GainsFamilyDto
                    {
                        Name = request.Name,
                        YearOfBirth = request.YearOfBirth,
                        Relationship = request.Relationship,
                    });

                    if (request.Id == null)
                    {
                        var gainsFamily = new GainsFamily()
                        {
                            GainsId = id,
                            Name = request.Name,
                            YearOfBirth = request.YearOfBirth,
                            Relationship = request.Relationship,
                            DeleteFlag = false,
                            CreatedApplicationUserId = user,
                            LastModifiedApplicationUserId = user,
                            CreatedDate = DateTime.Now,
                            LastModifiedDate = DateTime.Now
                        };

                        await context.GainsFamilies.AddAsync(gainsFamily);
                    }
                    else
                    {
                        var isExistsFamily = await context.GainsFamilies.FirstOrDefaultAsync(x => x.Id == request.Id && x.DeleteFlag != true && x.GainsId == id, cancellationToken);
                        if (isExistsFamily == null)
                            throw new ApplicationException($"Không có dữ liệu Id Family: {request.Id}.");

                        isExistsFamily.Name = request.Name;
                        isExistsFamily.Relationship = request.Relationship;
                        isExistsFamily.YearOfBirth = request.YearOfBirth;
                        isExistsFamily.LastModifiedApplicationUserId = user;
                        isExistsFamily.LastModifiedDate = DateTime.Now;

                        context.GainsFamilies.Update(isExistsFamily);
                    }
                }

                return true;
            }
            catch (Exception ex) { return false; }
        }
    }
}
