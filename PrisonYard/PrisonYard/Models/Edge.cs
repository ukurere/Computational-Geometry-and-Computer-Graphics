using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrisonYard.Models.Algorithm;
using PrisonYard.Models.Geometry;
using PrisonYard.Services.Demo;
using PrisonYard.Services.Parsing;
using PrisonYard.Services.Rendering;

namespace PrisonYard.Models;

public readonly record struct Edge(Vertex Start, Vertex End);