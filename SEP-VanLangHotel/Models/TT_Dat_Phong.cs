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
    
    public partial class TT_Dat_Phong
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TT_Dat_Phong()
        {
            this.TT_Doi_Phong = new HashSet<TT_Doi_Phong>();
        }
    
        public string Ma_TT_Dat_Phong { get; set; }
        public string Ho_Ten_KH { get; set; }
        public string CMND_CCCD_KH { get; set; }
        public string SDT_KH { get; set; }
        public System.DateTime Sinh_Nhat_KH { get; set; }
        public int Gioi_Tinh_KH { get; set; }
        public string Dia_Chi_KH { get; set; }
        public int Doi_Tra { get; set; }
        public System.DateTime Thoi_Gian_Dat { get; set; }
        public System.DateTime Thoi_Gian_Doi_Tra { get; set; }
        public string Ma_Phong { get; set; }
        public string Ma_Tai_Khoan { get; set; }
    
        public virtual Phong Phong { get; set; }
        public virtual Tai_Khoan Tai_Khoan { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TT_Doi_Phong> TT_Doi_Phong { get; set; }
    }
}
