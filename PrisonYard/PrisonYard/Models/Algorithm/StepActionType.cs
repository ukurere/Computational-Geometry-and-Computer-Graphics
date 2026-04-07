namespace PrisonYard.Models.Algorithm;

public enum StepActionType
{
    None = 0,

    InputPolygon,
    DetectOrientation,
    DetectReflexVertices,
    DetectHorizontalEdges,
    DetectPeaks,

    AddPartitionDiagonal,
    BuildMonotonePiece,

    BuildPyramid,
    AddQuadrangulationDiagonal,
    BuildQuadrilateral,

    ColorVertices,
    SelectGuardColor,
    FinalSolution
}