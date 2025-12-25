using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Models.Lession;
namespace Sale_Saas.Application.Features.LessionFeature.Commands;

public record Lession_AddOrUpdateCommand(Guid UserId, LessionAddOrUpdateRequest RequestData) : IRequest<Result<string>>;

public class Lession_AddOrUpdateCommandHandler : IRequestHandler<Lession_AddOrUpdateCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;
    private readonly IInternalService _internalService;
    private readonly IFileStorageService _storageService;

    public Lession_AddOrUpdateCommandHandler(IApplicationDbContext context, IEventLogService eventLogService, IInternalService internalService, IFileStorageService storageService)
    {
        _context = context;
        _eventLogService = eventLogService;
        _internalService = internalService;
        _storageService = storageService;
    }

    public async Task<Result<string>> Handle(Lession_AddOrUpdateCommand request, CancellationToken cancellationToken)
    {
        string result = string.Empty;

        if (request.RequestData.Id == null)
        {
            var sortOrder = await HandleSortOrderWithCaseCreate(request.RequestData, cancellationToken);

            // add
            if (request.RequestData.Link != null)
            {

                Lessions lession = new Lessions()
                {
                    Id = new Guid(),
                    Name = request.RequestData.Name,
                    Description = "",
                    Type = "LINK",
                    IsFile = false,
                    Link = request.RequestData.Link,
                    UnitId = request.RequestData.UnitId,
                    SortOrder = sortOrder,
                    CreatedDate = DateTime.Now,
                    CreatedApplicationUserId = request.UserId
                };

                _context.Lessions.Add(lession);

            }
            else if (request.RequestData.File != null)
            {
                var storages = await _storageService.UploadFileAsync(new List<IFormFile> { request.RequestData.File });

                if (storages.Count == 0)
                {
                    throw new ApplicationException("Tạo tài liệu bài học thất bại");
                }

                foreach (var item in storages)
                {
                    Lessions lessions = new Lessions()
                    {
                        Id = new Guid(),
                        Name = request.RequestData.Name,
                        Description = "",
                        FolderName = "",
                        OriginalFileName = item.OriginalFileName,
                        FileName = item.FileName,
                        ContentType = item.ContentType,
                        FileSize = item.FileSize,
                        FilePath = item.FilePath,
                        ServerPath = item.ServerPath,
                        Extension = item.Extension,
                        Type = GetFileTypeFromContentType(item.ContentType),
                        IsFile = true,
                        UnitId = request.RequestData.UnitId,
                        SortOrder = sortOrder,
                        CreatedDate = DateTime.Now,
                        CreatedApplicationUserId = request.UserId
                    };

                    _context.Lessions.Add(lessions);
                }
            }
            else
            {
                throw new ApplicationException("Không có dữ liệu file và link");
            }
        }
        else
        {
            // update
            var lesson = await _context.Lessions.Where(l => l.Id == request.RequestData.Id
                                                         && l.DeleteFlag != true)
                                                .FirstOrDefaultAsync();

            if (lesson == null)
            {
                throw new ApplicationException("Không tìm thấy bài học");
            }

            if (request.RequestData.File != null)
            {
                var storages = await _storageService.UploadFileAsync(new List<IFormFile> { request.RequestData.File });

                if (storages.Count == 0)
                {
                    throw new ApplicationException("Cập nhật tài liệu bài học thất bại");
                }

                foreach (var item in storages)
                {
                    lesson.OriginalFileName = item.OriginalFileName;
                    lesson.FileName = item.FileName;
                    lesson.ContentType = item.ContentType;
                    lesson.FileSize = item.FileSize;
                    lesson.FilePath = item.FilePath;
                    lesson.ServerPath = item.ServerPath;
                    lesson.Extension = item.Extension;
                    lesson.Type = GetFileTypeFromContentType(item.ContentType);
                };
            }

            if (request.RequestData.Name != null)
            {
                lesson.Name = request.RequestData.Name;
            }

            lesson.LastModifiedApplicationUserId = request.UserId;
            lesson.LastModifiedDate = DateTime.Now;

            _context.Lessions.Update(lesson);
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _eventLogService.Create("LessionFeature", "LessionFeature", "Lession_AddOrUpdateCommand", request.UserId);

        return Result<string>.Success(result);
    }

    private string GetFileTypeFromContentType(string contentType)
    {
        return ContentTypeToFileType.TryGetValue(contentType, out var fileType) ? fileType : "UNKNOWN";
    }

    private readonly Dictionary<string, string> ContentTypeToFileType = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
    {
        { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "docx" },
        { "application/pdf", "pdf" },
        { "application/msword", "docx" },
        { "application/vnd.ms-powerpoint", "ppt" },
        { "application/vnd.openxmlformats-officedocument.presentationml.presentation", "ppt" },
        { "video/mp4", "VIDEO" },
        { "video/x-msvideo", "VIDEO" },

        //{ "application/vnd.ms-excel", "EXCEL" },
        //{ "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EXCEL" },
        //{ "text/csv", "EXCEL" },
        //{ "audio/mpeg", "MP3 Audio" },
        //{ "audio/wav", "WAV Audio" },
        //{ "application/x-rar-compressed", "RAR Archive" },
        //{ "apVideotion/x-tar", "TAR Archive" },
        //{ "application/xhtml+xml", "XHTML" },
        //{ "application/xml", "XML" },
        //{ "text/html", "HTML File" },
        //{ "image/gif", "GIF Image" },
        //{ "image/bmp", "BMP Image" },
        //{ "image/tiff", "TIFF Image" },
        //{ "image/webp", "WEBP Image" },
        //{ "application/json", "JSON" },
        //{ "application/javascript", "JavaScript" },
        //{ "application/x-shockwave-flash", "Flash" },
        //{ "application/x-www-form-urlencoded", "Form Data" }
        //{ "image/jpeg", "JPEG Image" },
        //{ "image/png", "PNG Image" },
        //{ "text/plain", "Text File" },
        //{ "application/zip", "ZIP Archive" },
    };

    public async Task<int> HandleSortOrderWithCaseCreate(LessionAddOrUpdateRequest request, CancellationToken cancellationToken)
    {
        var count = await _context.Lessions.CountAsync(x => x.DeleteFlag != true && x.UnitId == request.UnitId);

        if (request.SortOrder == null || request.SortOrder <= 0 || request.SortOrder >= count + 1)
        {
            return count + 1;
        }
        else
        {
            // 1 <= sortOrder <= n
            var listLessons = await _context.Lessions.Where(s => s.DeleteFlag != true
                                                              && s.UnitId == request.UnitId
                                                              && s.SortOrder >= request.SortOrder)
                                                     .OrderBy(s => s.SortOrder)
                                                     .ToListAsync();

            var index = request.SortOrder + 1;
            foreach (var item in listLessons)
            {
                item.SortOrder = index;
                index = index + 1;
            }

            _context.Lessions.UpdateRange(listLessons);

            await _context.SaveChangesAsync(cancellationToken);

            return request.SortOrder ?? count + 1;
        }
    }
}

