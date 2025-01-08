namespace RevitTranslator.Enums;

/// <summary>
/// Represents translation attribute for elements that have multiple text properties
/// </summary>
public enum TranslationDetails
{
    /// <summary>
    /// Default value
    /// </summary>
    None,

    /// <summary>
    /// User-visible name of the element
    /// </summary>
    ElementName,

    /// <summary>
    /// "Above" override of dimension
    /// </summary>
    DimensionAbove,

    /// <summary>
    /// "Below" override of dimension
    /// </summary>
    DimensionBelow,

    /// <summary>
    /// Prefix override of dimension
    /// </summary>
    DimensionPrefix,

    /// <summary>
    /// Suffix override of dimension
    /// </summary>
    DimensionSuffix,

    /// <summary>
    /// CurrentValue override of dimension
    /// </summary>
    DimensionOverride,

    /// <summary>
    /// Address field in the document
    /// </summary>
    ProjectInfoAddress,

    /// <summary>
    /// Author field in the document
    /// </summary>
    ProjectInfoAuthor,

    /// <summary>
    /// Building Name field in the document
    /// </summary>
    ProjectInfoBuildingName,

    /// <summary>
    /// Client Name field in the document
    /// </summary>
    ProjectInfoClientName,

    /// <summary>
    /// Issue Date in the document
    /// </summary>
    ProjectInfoIssueDate,

    /// <summary>
    /// Name of the project
    /// </summary>
    ProjectInfoName,

    /// <summary>
    /// Number of the project
    /// </summary>
    ProjectInfoNumber,

    /// <summary>
    /// Organization Description in the document
    /// </summary>
    ProjectInfoOrganizationDescription,

    /// <summary>
    /// Organization Name in the document
    /// </summary>
    ProjectInfoOrganizationName,

    /// <summary>
    /// Status of the project
    /// </summary>
    ProjectInfoStatus,
}
