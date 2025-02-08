using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FluentUI.AdventureWorks.Server.Data.Models;

/// <summary>
/// Cross-reference table mapping products to related product documents.
/// </summary>
public partial class ProductDocument
{
    /// <summary>
    /// Product identification number. Foreign key to Product.ProductID.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Document identification number. Foreign key to Document.DocumentNode.
    /// </summary>
    public HierarchyId DocumentNode { get; set; } = null!;

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    public virtual Document DocumentNodeNavigation { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
