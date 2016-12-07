#region License

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at
//  
// http://www.apache.org/licenses/LICENSE-2.0.html
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  
// Copyright (c) 2013, HTW Berlin

#endregion

using System;
using Microsoft.Maps.MapControl.WPF;

namespace Ork.CarbonFootprint
{
  internal static class Utils
  {
    internal static Location CalculateMidPoint(double startlat, double startlong, double destinationlat, double destinationlong)
    {
      var midPoint = new Location();

      var dLon = DegreesToRadians(destinationlong - startlong);
      var Bx = Math.Cos(DegreesToRadians(destinationlat)) * Math.Cos(dLon);
      var By = Math.Cos(DegreesToRadians(destinationlat)) * Math.Sin(dLon);

      midPoint.Latitude =
        RadiansToDegrees(Math.Atan2(Math.Sin(DegreesToRadians(startlat)) + Math.Sin(DegreesToRadians(destinationlat)),
          Math.Sqrt((Math.Cos(DegreesToRadians(startlat)) + Bx) * (Math.Cos(DegreesToRadians(startlat)) + Bx) + By * By)));

      midPoint.Longitude = startlong + RadiansToDegrees(Math.Atan2(By, Math.Cos(DegreesToRadians(startlat)) + Bx));

      return midPoint;
    }

    private static double DegreesToRadians(double angle)
    {
      return Math.PI * angle / 180.0;
    }

    private static double RadiansToDegrees(double angle)
    {
      return angle * (180.0 / Math.PI);
    }
  }
}