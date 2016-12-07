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

namespace Ork.Map
{
  internal static class Orientation
  {
    internal static void SetScaledPosition(ref double topSize, ref double leftSize, double itemHeight, double itemWidth, double zoom, Alignment alignment)
    {
      if (alignment == Alignment.Center)
      {
        topSize = topSize - ((itemHeight / 2) / zoom);
        leftSize = leftSize - ((itemWidth / 2) / zoom);
      }
    }
  }
}