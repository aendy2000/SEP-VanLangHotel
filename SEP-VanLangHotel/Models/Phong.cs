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
    
    public partial class Phong
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Phong()
        {
            this.TT_Dat_Phong = new HashSet<TT_Dat_Phong>();
            this.TT_Doi_Phong = new HashSet<TT_Doi_Phong>();
            this.Coc_Phong = new HashSet<Coc_Phong>();
        }
    
        public string Ma_Phong { get; set; }
        public string So_Phong { get; set; }
        public string Hinh_Anh { get; set; }
        public string Ma_Loai_Phong { get; set; }
        public string Ma_Khach_San { get; set; }
        public string Ma_Trang_Thai { get; set; }
    
        public virtual Khach_San Khach_San { get; set; }
        public virtual Loai_Phong Loai_Phong { get; set; }
        public virtual Trang_Thai Trang_Thai { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TT_Dat_Phong> TT_Dat_Phong { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TT_Doi_Phong> TT_Doi_Phong { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Coc_Phong> Coc_Phong { get; set; }
    }
}
