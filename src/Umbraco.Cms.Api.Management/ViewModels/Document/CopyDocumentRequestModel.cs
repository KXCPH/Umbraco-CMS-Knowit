﻿namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class CopyDocumentRequestModel
{
    public ReferenceByIdRequestModel? Target { get; set; }

    public bool RelateToOriginal { get; set; }

    public bool IncludeDescendants { get; set; }
}
