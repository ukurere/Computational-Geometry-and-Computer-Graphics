using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrisonYard.Models;

public readonly record struct Edge(Vertex Start, Vertex End);