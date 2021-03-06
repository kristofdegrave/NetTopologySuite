﻿using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Noding;

namespace NetTopologySuite.Tests.NUnit.Noding
{
    public static class NodingTestUtility
    {
        public static Geometry ToLines(ICollection<ISegmentString> nodedList,
            GeometryFactory geomFact)
        {
            var lines = new LineString[nodedList.Count];
            int i = 0;
            foreach (NodedSegmentString nss in nodedList)
            {
                var pts = nss.Coordinates;
                var line = geomFact.CreateLineString(pts);
                lines[i++] = line;
            }

            if (lines.Length == 1) return lines[0];
            return geomFact.CreateMultiLineString(lines);
        }

        public static IList<ISegmentString> ToSegmentStrings(IEnumerable<Geometry> lines)
        {
            var nssList = new List<ISegmentString>();
            foreach (LineString line in lines)
            {
                var nss = new NodedSegmentString(line.Coordinates, line);
                nssList.Add(nss);
            }
            return nssList;
        }

        public static ICollection<ISegmentString> GetNodedSubstrings(NodedSegmentString nss)
        {
            var resultEdgelist = new List<ISegmentString>();
            nss.NodeList.AddSplitEdges(resultEdgelist);
            return resultEdgelist;
        }
        /**
         * Runs a noder on one or two sets of input geometries
         * and validates that the result is fully noded.
         * 
         * @param geom1 a geometry
         * @param geom2 a geometry, which may be null
         * @param noder the noder to use
         * @return the fully noded linework
         * 
         * @throws TopologyException
         */
        public static Geometry NodeValidated(Geometry geom1, Geometry geom2, INoder noder)
        {
            var lines = new List<Geometry>(LineStringExtracter.GetLines(geom1));
            if (geom2 != null)
            {
                lines.AddRange(LineStringExtracter.GetLines(geom2));
            }
            var ssList = ToSegmentStrings(lines);

            var noderValid = new ValidatingNoder(noder);
            noderValid.ComputeNodes(ssList);
            var nodedList = noder.GetNodedSubstrings();

            var result = ToLines(nodedList, geom1.Factory);
            return result;
        }


        public static NodedSegmentString CreateNSS(params double[] ords)
        {
            if (ords.Length % 2 != 0)
            {
                throw new ArgumentException("Must provide pairs of ordinates");
            }
            var pts = new Coordinate[ords.Length / 2];
            for (int i = 0; i <= ords.Length; i += 2)
            {
                var p = new Coordinate(ords[i], ords[i + 1]);
                pts[i / 2] = p;
            }
            var nss = new NodedSegmentString(pts, null);
            return nss;
        }

    }
}
