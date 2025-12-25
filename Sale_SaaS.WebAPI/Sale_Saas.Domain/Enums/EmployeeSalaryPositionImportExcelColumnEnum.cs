using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Domain.Enums;

public enum EmployeeSalaryPositionImportExcelColumnEnum
{
    TenantId = 1,
    Email = 2,
    Month = 3,
    Year = 4,
    RoleId_1 = 5,
    Income_1 = 6,
    RoleId_2 = 7,
    Income_2 = 8,
    RoleId_3 = 9,
    Income_3 = 10,
    RoleId_4 = 11,
    Income_4 = 12,
    RoleId_5 = 13,
    Income_5 = 14,
    IncomeOther = 15, // CacKhoanThuNhapKhac
    IncomeBeforeTax = 16, // TongThuNhapTruocThue
    IncomeNonTax = 17, // TongThuNhapKhongChiuThue
    Dependent = 18, // TongGiamTruGiaCanh
    Insurance = 19, // TongTienBHXHNLDDongThang
    IncomeTax = 20, // TongThuNhapChiuThue
    PersonalIncomeTax = 21, // TongThueTNCNTamThu
    IncomeReceived = 22 // TongThuNhapNhanDuoc
}
