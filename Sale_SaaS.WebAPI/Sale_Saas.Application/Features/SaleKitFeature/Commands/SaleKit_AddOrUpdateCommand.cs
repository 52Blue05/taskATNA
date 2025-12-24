using Sale_Saas.Application.Features.SaleKitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.SaleKit;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.SaleKitFeature.Commands
{
    public record SaleKit_AddOrUpdateCommand(Guid UserId, SaleKitAddRequest RequestData) : IRequest<Result<List<SaleKitDto>>>;

    public class SaleKit_AddOrUpdateCommandHandler : IRequestHandler<SaleKit_AddOrUpdateCommand, Result<List<SaleKitDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IApplicationRoleService _roleService;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IFileStorageService _storageService;

        public SaleKit_AddOrUpdateCommandHandler(IMapper mapper,
                                                IApplicationRoleService roleService,
                                                IApplicationDbContext context,
                                                IInternalService internalService,
                                                IFileStorageService storageService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
            _storageService = storageService;
            _roleService = roleService;
        }

        public async Task<Result<List<SaleKitDto>>> Handle(SaleKit_AddOrUpdateCommand request, CancellationToken cancellationToken)
        {
            if (request.RequestData.files.Count < 0)
            {
                throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
            }

            var listRoleOfUser = await _roleService.GetListRoleByUserId(request.UserId);

            if (request.RequestData.ParentId == Guid.Empty) request.RequestData.ParentId = null;
            if (request.RequestData.ParentId != null)
            {
                var parent = await _context.SaleKits.FirstOrDefaultAsync(s => s.Id == request.RequestData.ParentId);
                if (parent == null) throw new ApplicationException($"Không tìm thấy dữ liệu với Id: {request.RequestData.ParentId}");
            }
            List<SaleKitDto> salekits = new List<SaleKitDto>();
            if (request.RequestData.Type == SaleKitTypeEnum.FILE.ToString())
            {
                foreach (var file in request.RequestData.files)
                {
                    //var storages = await _storageService.UploadFileAsync(new List<IFormFile> { file }, request.RequestData.Folder ?? "sass");
                    var storages = await _storageService.UploadFileAsync(new List<IFormFile> { file });
                    if (storages.Count == 0)
                    {
                        throw new ApplicationException($"Tạo tài liệu sale kit thất bại");
                    }
                    foreach (var item in storages)
                    {
                        SaleKit salekit = new SaleKit()
                        {
                            Id = new Guid(),
                            Name = item.OriginalFileName,
                            Description = "",
                            OriginalFileName = item.OriginalFileName,
                            FileName = item.FileName,
                            FileSize = item.FileSize,
                            ContentType = item.ContentType,
                            FilePath = item.FilePath,
                            ServerPath = item.ServerPath,
                            Extension = item.Extension,
                            FolderName = item.FolderName,
                            ParentId = request.RequestData.ParentId,
                            Type = GetFileTypeFromContentType(item.ContentType),
                            DeleteFlag = false,
                            CreatedApplicationUserId = request.RequestData.ApplicationUserId,
                            LastModifiedApplicationUserId = request.RequestData.ApplicationUserId,
                            CreatedDate = DateTime.Now,
                            LastModifiedDate = DateTime.Now
                        };
                        await _context.SaleKits.AddAsync(salekit);

                        var listSaleKitByRole = listRoleOfUser.Select(s => new ApplicationRoleSaleKit()
                        {
                            Access = true,
                            ApplicationRoleId = s.Id,
                            SaleKitId = salekit.Id,
                            CreatedApplicationUserId = request.UserId,
                            LastModifiedApplicationUserId = request.UserId
                        });

                        _context.ApplicationRole_SaleKits.AddRange(listSaleKitByRole);

                        salekits.Add(_mapper.Map<SaleKitDto>(salekit));
                    }
                }
            }
            else if (request.RequestData.Type == SaleKitTypeEnum.FOLDER.ToString())
            {
                SaleKit salekit = new SaleKit()
                {
                    Id = new Guid(),
                    Name = request.RequestData.Name,
                    Description = "",
                    OriginalFileName = request.RequestData.Name,
                    FileName = request.RequestData.Name,
                    ParentId = request.RequestData.ParentId,
                    Type = request.RequestData.Type,
                    DeleteFlag = false,
                    CreatedApplicationUserId = request.RequestData.ApplicationUserId,
                    LastModifiedApplicationUserId = request.RequestData.ApplicationUserId,
                    CreatedDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now
                };
                await _context.SaleKits.AddAsync(salekit);

                var listSaleKitByRole = listRoleOfUser.Select(s => new ApplicationRoleSaleKit()
                {
                    Access = true,
                    ApplicationRoleId = s.Id,
                    SaleKitId = salekit.Id,
                    CreatedApplicationUserId = request.UserId,
                    LastModifiedApplicationUserId = request.UserId
                });

                _context.ApplicationRole_SaleKits.AddRange(listSaleKitByRole);

                salekits.Add(_mapper.Map<SaleKitDto>(salekit));
            }
            else
            {
                throw new ApplicationException("Loại tệp không hợp lệ");
            }


            await _context.SaveChangesAsync(cancellationToken);
            _context.ClearChangeTracker();

            return Result<List<SaleKitDto>>.Success(salekits);
        }

        private string GetFileTypeFromContentType(string contentType)
        {
            return ContentTypeToFileType.TryGetValue(contentType, out var fileType) ? fileType : "UNKNOWN";
        }

        private readonly Dictionary<string, string> ContentTypeToFileType = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "DOCX" },
            { "application/pdf", "PDF" },
            { "application/msword", "DOCX" },
            { "application/vnd.ms-powerpoint", "PPT" },
            { "application/vnd.openxmlformats-officedocument.presentationml.presentation", "PPT" },
			//{ "video/mp4", "VIDEO" },
			//{ "video/x-msvideo", "VIDEO" },
			{ "application/vnd.ms-excel", "XLSX" },
            { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "XLSX" },
            { "text/csv", "XLSX" },
			//{ "audio/mpeg", "MP3 Audio" },
			//{ "audio/wav", "WAV Audio" },
			//{ "application/x-rar-compressed", "RAR Archive" },
			//{ "apVideotion/x-tar", "TAR Archive" },
			//{ "application/xhtml+xml", "XHTML" },
			//{ "application/xml", "XML" },
			//{ "text/html", "HTML File" },
			{ "image/gif", "GIF Image" },
            { "image/bmp", "BMP Image" },
            { "image/tiff", "TIFF Image" },
            { "image/webp", "WEBP Image" },
			//{ "application/json", "JSON" },
			//{ "application/javascript", "JavaScript" },
			//{ "application/x-shockwave-flash", "Flash" },
			//{ "application/x-www-form-urlencoded", "Form Data" }
			{ "image/jpeg", "JPEG Image" },
            { "image/png", "PNG Image" },
            { "text/plain", "Text File" },
			//{ "application/zip", "ZIP Archive" },
		};
    }
}
