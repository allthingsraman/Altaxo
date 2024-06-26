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

#nullable disable warnings

namespace Altaxo.Worksheet.Commands
{
  public class PLSQuestPreferredNumberOfFactors : AbstractWorksheetControllerCommand
  {
    public override void Run(Altaxo.Gui.Worksheet.Viewing.WorksheetController ctrl)
    {
      Altaxo.Worksheet.Commands.Analysis.ChemometricCommands.QuestPreferredNumberOfFactors(ctrl.DataTable);
    }
  }

  public class PLSPlotPredictedVersusActualYIndividually : AbstractWorksheetControllerCommand
  {
    public override void Run(Altaxo.Gui.Worksheet.Viewing.WorksheetController ctrl)
    {
      Altaxo.Worksheet.Commands.Analysis.ChemometricCommands.PlotPredictedVersusActualY(ctrl.DataTable);
    }
  }

  public class PLSPlotCrossPredictedVersusActualYIndividually : AbstractWorksheetControllerCommand
  {
    public override void Run(Altaxo.Gui.Worksheet.Viewing.WorksheetController ctrl)
    {
      Altaxo.Worksheet.Commands.Analysis.ChemometricCommands.PlotCrossPredictedVersusActualY(ctrl.DataTable, true);
    }
  }

  public class PLSPlotYResidualsIndividually : AbstractWorksheetControllerCommand
  {
    public override void Run(Altaxo.Gui.Worksheet.Viewing.WorksheetController ctrl)
    {
      Altaxo.Worksheet.Commands.Analysis.ChemometricCommands.PlotYResiduals(ctrl.DataTable);
    }
  }

  public class PLSPlotYCrossResidualsIndividually : AbstractWorksheetControllerCommand
  {
    public override void Run(Altaxo.Gui.Worksheet.Viewing.WorksheetController ctrl)
    {
      Altaxo.Worksheet.Commands.Analysis.ChemometricCommands.PlotYCrossResiduals(ctrl.DataTable);
    }
  }

  public class PLSPlotXResidualsIndividually : AbstractWorksheetControllerCommand
  {
    public override void Run(Altaxo.Gui.Worksheet.Viewing.WorksheetController ctrl)
    {
      Altaxo.Worksheet.Commands.Analysis.ChemometricCommands.PlotXResiduals(ctrl.DataTable);
    }
  }

  public class PLSPlotXCrossResidualsIndividually : AbstractWorksheetControllerCommand
  {
    public override void Run(Altaxo.Gui.Worksheet.Viewing.WorksheetController ctrl)
    {
      Altaxo.Worksheet.Commands.Analysis.ChemometricCommands.PlotXCrossResiduals(ctrl.DataTable);
    }
  }

  public class PLSPlotPRESSValue : AbstractWorksheetControllerCommand
  {
    public override void Run(Altaxo.Gui.Worksheet.Viewing.WorksheetController ctrl)
    {
      Altaxo.Worksheet.Commands.Analysis.ChemometricCommands.PlotPRESSValue(ctrl.DataTable);
    }
  }

  public class PLSPlotCrossPRESSValue : AbstractWorksheetControllerCommand
  {
    public override void Run(Altaxo.Gui.Worksheet.Viewing.WorksheetController ctrl)
    {
      Altaxo.Worksheet.Commands.Analysis.ChemometricCommands.PlotCrossPRESSValue(ctrl.DataTable);
    }
  }

  public class PLSPlotXLeverage : AbstractWorksheetControllerCommand
  {
    public override void Run(Altaxo.Gui.Worksheet.Viewing.WorksheetController ctrl)
    {
      Altaxo.Worksheet.Commands.Analysis.ChemometricCommands.PlotXLeverage(ctrl.DataTable);
    }
  }

  public class PLSPlotPreprocessedSpectra : AbstractWorksheetControllerCommand
  {
    public override void Run(Altaxo.Gui.Worksheet.Viewing.WorksheetController ctrl)
    {
      Altaxo.Worksheet.Commands.Analysis.ChemometricCommands.PlotPreprocessedSpectra(ctrl.DataTable);
    }
  }

  public class PLSPlotPredictionScores : AbstractWorksheetControllerCommand
  {
    public override void Run(Altaxo.Gui.Worksheet.Viewing.WorksheetController ctrl)
    {
      Altaxo.Worksheet.Commands.Analysis.ChemometricCommands.PlotPredictionScores(ctrl.DataTable);
    }
  }
}
