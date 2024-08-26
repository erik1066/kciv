/*
 Copyright (c) 2003-2016 Niels Kokholm, Peter Sestoft, and Rasmus Lystrøm
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/

using System;
using System.Text;

namespace OpenCiv.Engine.Collections
{
    /// <summary>
    /// <i>(Describe usage of "L:300" format string.)</i>
    /// </summary>
    public interface IShowable : IFormattable
    {
        //TODO: wonder if we should use TextWriters instead of StringBuilders?
        /// <summary>
        /// Format <code>this</code> using at most approximately <code>rest</code> chars and 
        /// append the result, possibly truncated, to stringbuilder.
        /// Subtract the actual number of used chars from <code>rest</code>.
        /// </summary>
        /// <param name="stringbuilder"></param>
        /// <param name="rest"></param>
        /// <param name="formatProvider"></param>
        /// <returns>True if the appended formatted string was complete (not truncated).</returns>
        bool Show(StringBuilder stringbuilder, ref int rest, IFormatProvider formatProvider);
    }
    // ------------------------------------------------------------
    
}