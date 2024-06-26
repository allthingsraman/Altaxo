﻿#region Copyright

/////////////////////////////////////////////////////////////////////////////
//    Altaxo:  a data processing and data plotting program
//    Copyright (C) 2002-2011 Dr. Dirk Lellinger
//
//    This program is free software; you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation; either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program; if not, write to the Free Software
//    Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
//
/////////////////////////////////////////////////////////////////////////////

#endregion Copyright

#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Altaxo.Main.Services.ScriptCompilation;

namespace Altaxo.Scripting
{
  /// <summary>
  /// Script for a program instance of Altaxo. Main purpose is add functionality, when the script must load or close projects.
  /// </summary>

  public class ProgramInstanceScript : AbstractScript, IScriptText
  {
    /// <summary>
    /// Creates an empty  script.
    /// </summary>
    public ProgramInstanceScript()
    {
    }

    /// <summary>
    /// Creates a script as a copy from another script.
    /// </summary>
    /// <param name="b">The script to copy from.</param>
    public ProgramInstanceScript(ProgramInstanceScript b)
      : this(b, true)
    {
    }

    /// <summary>
    /// Creates a  script as a copy from another script with the option for modifying the txt.
    /// </summary>
    /// <param name="b">The script to copy from.</param>
    /// <param name="forModification">If true, the new script text can be modified.</param>
    public ProgramInstanceScript(ProgramInstanceScript b, bool forModification)
      : base(b, forModification)
    {
    }

    /// <summary>
    /// Gives the type of the script object (full name), which is created after successfull compilation.
    /// </summary>
    public override string ScriptObjectType
    {
      get { return "Altaxo.Scripting.ProgramInstanceScriptObject"; }
    }

    /// <summary>
    /// Gets the code header, i.e. the leading script text. It depends on the ScriptStyle.
    /// </summary>
    public override string CodeHeader
    {
      get
      {
        return
          "#region ScriptHeader\r\n" +
          "using System;\r\n" +
          "using System.Collections.Generic;\r\n" +
          "using System.Linq;\r\n" +
          "using Altaxo;\r\n" +
          "using Altaxo.Calc;\r\n" +
          "using Altaxo.Data;\r\n" +
          "using Altaxo.Main;\r\n" +
          "namespace Altaxo.Scripting\r\n" +
          "{\r\n" +
          "\tpublic class ProgramInstanceScriptObject : Altaxo.Calc.ProgramInstanceExeBase\r\n" +
          "\t{\r\n" +
          "\t\tpublic override void Execute(IProgressReporter reporter)\r\n" +
          "\t\t{\r\n"
          ;
      }
    }

    public override string CodeStart
    {
      get
      {
        return
          "#endregion\r\n" +
          "\t\t\t// ----- add your script below this line -----\r\n";
      }
    }

    public override string CodeUserDefault
    {
      get
      {
        return
          "\t\t\t\r\n" +
          "\t\t\t// you can use the Current class to get access to the project and project service\r\n" +
          "\t\t\t// for instance: Current.Project   or   Current.ProjectService\r\n" +
          "\t\t\t\r\n"
          ;
      }
    }

    public override string CodeEnd
    {
      get
      {
        return
          "\t\t\t// ----- add your script above this line -----\r\n" +
          "#region ScriptFooter\r\n";
      }
    }

    /// <summary>
    /// Get the ending text of the script, dependent on the ScriptStyle.
    /// </summary>
    public override string CodeTail
    {
      get
      {
        return
          "\t\t} // Execute method\r\n" +
          "\t} // class\r\n" +
          "} //namespace\r\n" +
          "#endregion\r\n";
      }
    }

    /// <summary>
    /// Clones the script.
    /// </summary>
    /// <returns>The cloned object.</returns>
    public override object Clone()
    {
      return new ProgramInstanceScript(this, true);
    }

    /// <summary>
    /// Executes the script. If no instance of the script object exists, a error message will be stored and the return value is false.
    /// If the script object exists, the Execute function of this script object is called.
    /// </summary>
    /// <returns>True if executed without exceptions, otherwise false.</returns>
    /// <remarks>If exceptions were thrown during execution, the exception messages are stored
    /// inside the column script and can be recalled by the Errors property.</remarks>
    public bool Execute(IProgressReporter reporter)
    {
      if (_scriptObject is null)
      {
        _errors = ImmutableArray.Create(new CompilerDiagnostic(null, null, DiagnosticSeverity.Error, "Script Object is null"));
        return false;
      }

      try
      {
        ((Altaxo.Calc.ProgramInstanceExeBase)_scriptObject).Execute(reporter);
      }
      catch (Exception ex)
      {
        _errors = ImmutableArray.Create(new CompilerDiagnostic(null, null, DiagnosticSeverity.Error, ex.ToString()));
        return false;
      }
      return true;
    }
  } // end of class
}
