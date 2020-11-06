#region PDFsharp - A .NET library for processing PDF
//
// Authors:
//   Stefan Lange
//
// Copyright (c) 2005-2017 empira Software GmbH, Cologne Area (Germany)
//
// http://www.pdfsharp.com
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using PdfSharp.Drawing;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.Internal;
using System;

namespace PdfSharp.Pdf.AcroForms
{
    /// <summary>
    /// Represents the combo box field.
    /// </summary>
    public sealed class PdfComboBoxField : PdfChoiceField
    {
        /// <summary>
        /// Initializes a new instance of PdfComboBoxField.
        /// </summary>
        internal PdfComboBoxField(PdfDocument document)
            : base(document)
        { }

        internal PdfComboBoxField(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Gets or sets the index of the selected item.
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                string value = Elements.GetString(PdfAcroField.Keys.V);
                // try export value first
                var index = IndexInOptArray(value, true);
                if (index < 0)
                    index = IndexInOptArray(value, false);
                return index;
            }
            set
            {
                string key = ValueInOptArray(value, true);
                Elements.SetString(PdfAcroField.Keys.V, key);
                RenderAppearance();
            }
        }

        /// <summary>
        /// Gets or sets the value of the field.
        /// </summary>
        // xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx		  
        public override PdfItem Value //R080304
        {
            get { return base.Value; }
            set { base.Value = value; RenderAppearance(); }
        }

        void RenderAppearance()
        {
            for (var i = 0; i < Annotations.Elements.Count; i++)
            {
                var widget = Annotations.Elements[i];
                if (widget == null)
                    continue;

                var rect = widget.Rectangle;
                var xRect = new XRect(0, 0, rect.Width, rect.Height);
                var form = new XForm(_document, xRect);
                EnsureFonts(form);
                using (var gfx = XGraphics.FromForm(form))
                {
                    if (widget.BackColor != XColor.Empty)
                        gfx.DrawRectangle(new XSolidBrush(widget.BackColor), xRect);
                    // Draw Border
                    if (!widget.BorderColor.IsEmpty)
                        gfx.DrawRectangle(new XPen(widget.BorderColor), xRect);

                    var index = SelectedIndex;
                    if (index > 0)
                    {
                        var text = ValueInOptArray(index, false);
                        if (!String.IsNullOrEmpty(text))
                        {
                            var format = TextAlign == TextAlignment.Left ? XStringFormats.CenterLeft : TextAlign == TextAlignment.Center ? XStringFormats.Center : XStringFormats.CenterRight;
                            gfx.DrawString(text, Font, new XSolidBrush(ForeColor), xRect, format);
                        }
                    }
                    form.DrawingFinished();

                    var ap = new PdfDictionary(this._document);
                    widget.Elements[PdfAnnotation.Keys.AP] = ap;
                    widget.Elements.SetName(PdfAnnotation.Keys.AS, "/N");   // set appearance state
                    // Set XRef to normal state
                    ap.Elements["/N"] = form.PdfForm.Reference;

                    var xobj = form.PdfForm;
                    var s = xobj.Stream.ToString();
                    s = "/Tx BMC\n" + s + "\nEMC";
                    xobj.Stream.Value = new RawEncoding().GetBytes(s);
                }
            }
        }

        internal override void Flatten()
        {
            base.Flatten();

            var index = SelectedIndex;
            if (index >= 0)
            {
                var text = ValueInOptArray(index, false);
                if (text.Length > 0)
                {
                    for (var i = 0; i < Annotations.Elements.Count; i++)
                    {
                        var widget = Annotations.Elements[i];
                        if (widget.Page != null)
                        {
                            var rect = widget.Rectangle;
                            if (!rect.IsEmpty)
                            {
                                var format = TextAlign == TextAlignment.Left ? XStringFormats.CenterLeft : TextAlign == TextAlignment.Center ? XStringFormats.Center : XStringFormats.CenterRight;
                                var xRect = new XRect(rect.X1, widget.Page.Height.Point - rect.Y2, rect.Width, rect.Height);
                                using (var gfx = XGraphics.FromPdfPage(widget.Page))
                                {
                                    gfx.Save();
                                    gfx.IntersectClip(xRect);
                                    // Note: Page origin [0,0] is bottom left !
                                    gfx.DrawString(text, Font, new XSolidBrush(ForeColor), xRect, format);
                                    gfx.Restore();
                                }
                            }
                        }
                    }
                }
            }
        }

        internal override void PrepareForSave()
        {
            base.PrepareForSave();
            RenderAppearance();
        }

        /// <summary>
        /// Predefined keys of this dictionary. 
        /// The description comes from PDF 1.4 Reference.
        /// </summary>
        public new class Keys : PdfAcroField.Keys
        {
            // Combo boxes have no additional entries.

            internal static DictionaryMeta Meta
            {
                get
                {
                    if (Keys._meta == null)
                        Keys._meta = CreateMeta(typeof(Keys));
                    return Keys._meta;
                }
            }
            static DictionaryMeta _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta
        {
            get { return Keys.Meta; }
        }
    }
}
