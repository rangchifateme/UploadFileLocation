//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UploadFileLocation.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class File
    {
        public int FileId { get; set; }
        public string Filename { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public Nullable<int> Fk_LocationId { get; set; }
    }
}
