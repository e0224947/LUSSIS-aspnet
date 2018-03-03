namespace LUSSIS.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Department")]
    public partial class Department
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Department()
        {
            Disbursements = new HashSet<Disbursement>();
            Employees = new HashSet<Employee>();
        }

        [Key]
        [StringLength(20)]
        [Display(Name = "Department Code")]
        public string DeptCode { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Department")]
        public string DeptName { get; set; }

        [Required]
        public int DeptHeadNum { get; set; }

        [Required]
        [StringLength(50)]
        public string ContactName { get; set; }

        [Required]
        [StringLength(30)]
        public string FaxNum { get; set; }

        [Required]
        [StringLength(30)]
        public string TelephoneNum { get; set; }

        public int? RepEmpNum { get; set; }

        public int? CollectionPointId { get; set; }

        public virtual CollectionPoint CollectionPoint { get; set; }

        public virtual Employee RepEmployee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Disbursement> Disbursements { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Employee> Employees { get; set; }
    }
}
