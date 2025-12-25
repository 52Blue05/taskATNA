using OfficeOpenXml;

namespace Sale_Saas.Application.Features.EmployeeSalaryDetailFeature.Commands
{
     public record class EmployeeSalaryDetail_ImportSampleCommand : IRequest<ExcelPackage>;

     public class EmployeeSalaryDetail_ImportSampleCommandHandler : IRequestHandler<EmployeeSalaryDetail_ImportSampleCommand, ExcelPackage>
     {
          public EmployeeSalaryDetail_ImportSampleCommandHandler()
          {

          }

          public async Task<ExcelPackage> Handle(EmployeeSalaryDetail_ImportSampleCommand request, CancellationToken cancellationToken)
          {

               ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
               ExcelPackage excel = new ExcelPackage();

               var workSheet = excel.Workbook.Worksheets.Add("Danh sách chi tiết thu nhập theo ticket");
               workSheet = ExcelExportHelper.GetStyle(workSheet, 7);

               workSheet.Cells[1, 1].Value = "Nội dung";
               workSheet.Cells[1, 2].Value = "Số tiền dự kiến";
               workSheet.Cells[1, 3].Value = "Số tiền thực chi";
               workSheet.Cells[1, 4].Value = "Đối tượng";
               workSheet.Cells[1, 5].Value = "Loại CP";
               workSheet.Cells[1, 6].Value = "Tên dự án";
               workSheet.Cells[1, 7].Value = "Thời điểm chi";

               int currRow = 2;

               for (var index = 1; index <= 5; index++)
               {
                    workSheet.Row(currRow).Height = 20;

                    workSheet.Cells[currRow, 1].Value = $"Nội dung thu nhập {index}";
                    workSheet.Cells[currRow, 2].Value = $"{index}00";
                    workSheet.Cells[currRow, 3].Value = $"{index}00";
                    workSheet.Cells[currRow, 4].Value = $"Loại đối tượng {index}";
                    workSheet.Cells[currRow, 5].Value = $"Loại CP {index}";
                    workSheet.Cells[currRow, 6].Value = $"Dự án {index}";
                    workSheet.Cells[currRow, 7].Value = $"04/02/2024";
                    currRow++;
               }

               workSheet.Cells.AutoFitColumns();

               return excel;
          }
     }
}
