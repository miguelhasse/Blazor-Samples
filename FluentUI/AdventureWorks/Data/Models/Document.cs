using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FluentUI.AdventureWorks.Server.Data.Models;

/// <summary>
/// Product maintenance documents.
/// </summary>
public partial class Document
{
    /// <summary>
    /// Primary key for Document records.
    /// </summary>
    public HierarchyId DocumentNode { get; set; } = null!;

    /// <summary>
    /// Depth in the document hierarchy.
    /// </summary>
    public short? DocumentLevel { get; set; }

    /// <summary>
    /// Title of the document.
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Employee who controls the document.  Foreign key to Employee.BusinessEntityID
    /// </summary>
    public int Owner { get; set; }

    /// <summary>
    /// 0 = This is a folder, 1 = This is a document.
    /// </summary>
    public bool FolderFlag { get; set; }

    /// <summary>
    /// File name of the document
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// File extension indicating the document type. For example, .doc or .txt.
    /// </summary>
    public string FileExtension { get; set; } = null!;

    /// <summary>
    /// Revision number of the document. 
    /// </summary>
    public string Revision { get; set; } = null!;

    /// <summary>
    /// Engineering change approval number.
    /// </summary>
    public int ChangeNumber { get; set; }

    /// <summary>
    /// 1 = Pending approval, 2 = Approved, 3 = Obsolete
    /// </summary>
    public byte Status { get; set; }

    /// <summary>
    /// Document abstract.
    /// </summary>
    public string? DocumentSummary { get; set; }

    /// <summary>
    /// Complete document.
    /// </summary>
    public byte[]? Document1 { get; set; }

    /// <summary>
    /// ROWGUIDCOL number uniquely identifying the record. Required for FileStream.
    /// </summary>
    public Guid Rowguid { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    public virtual Employee OwnerNavigation { get; set; } = null!;

    public virtual ICollection<ProductDocument> ProductDocuments { get; set; } = new List<ProductDocument>();
}
