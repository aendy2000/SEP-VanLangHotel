//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SEP_VanLangHotel.Models
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    public partial class Tai_Khoan
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Tai_Khoan()
        {
            this.TT_Dat_Phong = new HashSet<TT_Dat_Phong>();
        }
    
        public string Ma_Tai_Khoan { get; set; }
        public string Ten_Dang_Nhap { get; set; }
        public string Mat_Khau { get; set; }
        public int Lock { get; set; }
        public string Ma_Quyen { get; set; }
        public string Ma_Khach_San { get; set; }
        public string CMND_CCCD { get; set; }
        public string SDT { get; set; }
        public string Ho_Va_Ten { get; set; }
        public System.DateTime Sinh_Nhat { get; set; }
        public int Gioi_Tinh { get; set; }
        public string Dia_Chi { get; set; }
        public string Email { get; set; }
        public string Verify_Password { get; set; }
        public string OutOfDate_Code { get; set; }
        public string Avatar { get; set; }
    
        public virtual Khach_San Khach_San { get; set; }
        public virtual Quyen Quyen { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TT_Dat_Phong> TT_Dat_Phong { get; set; }
    }
}
