﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

// Originated from: Roslyn, Features, Core/Portable/SignatureHelp/ISignatureHelpProvider.cs

#if !NoCompletion && !NoSignatureHelp
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.SignatureHelp;


namespace Altaxo.CodeEditing.SignatureHelp
{
  public interface IAxoSignatureHelpProvider
  {
    bool IsTriggerCharacter(char ch);

    bool IsRetriggerCharacter(char ch);

    internal Task<SignatureHelpItems> GetItemsAsync(Document document, int position, SignatureHelpTriggerInfo triggerInfo, CancellationToken cancellationToken = default(CancellationToken));
  }
}
#endif
