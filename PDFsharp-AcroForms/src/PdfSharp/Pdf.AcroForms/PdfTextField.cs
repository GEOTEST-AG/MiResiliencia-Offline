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
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.Internal;
using System;

namespace PdfSharp.Pdf.AcroForms
{
    /// <summary>
    /// Represents the text field.
    /// </summary>
    public sealed class PdfTextField : PdfAcroField
    {
        /// <summary>
        /// Initializes a new instance of PdfTextField.
        /// </summary>
        internal PdfTextField(PdfDocument document)
            : base(document)
        { }

        internal PdfTextField(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Gets or sets the text value of the text field.
        /// </summary>
        public string Text
        {
            get { return Elements.GetString(Keys.V); }
            set { Elements.SetString(Keys.V, value); }
        }

        /// <summary>
        /// Gets or sets the maximum length of the field.
        /// </summary>
        /// <value>The length of the max.</value>
        public int MaxLength
        {
            get { return Elements.GetInteger(Keys.MaxLen); }
            set { Elements.SetInteger(Keys.MaxLen, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field has multiple lines.
        /// </summary>
        public bool MultiLine
        {
            get { return (Flags & PdfAcroFieldFlags.Multiline) != 0; }
            set
            {
                if (value)
                    SetFlags |= PdfAcroFieldFlags.Multiline;
                else
                    SetFlags &= ~PdfAcroFieldFlags.Multiline;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this field is used for passwords.
        /// </summary>
        public bool Password
        {
            get { return (Flags & PdfAcroFieldFlags.Password) != 0; }
            set
            {
                if (value)
                    SetFlags |= PdfAcroFieldFlags.Password;
                else
                    SetFlags &= ~PdfAcroFieldFlags.Password;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this field is a combined field.
        /// A combined field is a text field made up of multiple "combs" of equal width. The number of combs is determined by <see cref="MaxLength"/>.
        /// </summary>
        public bool Combined
        {
            get { return (Flags & PdfAcroFieldFlags.Comb) != 0; }
            set
            {
                if (value)
                    SetFlags |= PdfAcroFieldFlags.Comb;
                else
                    SetFlags &= ~PdfAcroFieldFlags.Comb;
            }
        }

        /// <summary>
        /// Creates the normal appearance form X object for the annotation that represents
        /// this acro form text field.
        /// </summary>
        private void RenderAppearance()
        {
            for (var i = 0; i < Annotations.Elements.Count; i++)
            {
                var widget = Annotations.Elements[i];
                if (widget == null)
                    continue;

                if ((widget.Flags & PdfAnnotationFlags.Invisible) != 0 || (widget.Flags & PdfAnnotationFlags.NoView) != 0)
                    continue;

                var rect = widget.Rectangle;
                var xRect = new XRect(0, 0, rect.Width, rect.Height);
                var form = (widget.Rotation == 90 || widget.Rotation == 270) && (widget.Flags & PdfAnnotationFlags.NoRotate) == 0 ? new XForm(_document, rect.Height, rect.Width) : new XForm(_document, xRect);
                EnsureFonts(form);
                using (var gfx = XGraphics.FromForm(form))
                {
                    var text = Text;
                    if (MaxLength > 0)
                        text = text.Substring(0, Math.Min(Text.Length, MaxLength));
                    var format = TextAlign == TextAlignment.Left ? XStringFormats.CenterLeft : TextAlign == TextAlignment.Center ? XStringFormats.Center : XStringFormats.CenterRight;
                    if (MultiLine)
                        format.LineAlignment = XLineAlignment.Near;
                    if (text.Length > 0)
                    {
                        if (widget.Rotation != 0 && (widget.Flags & PdfAnnotationFlags.NoRotate) == 0)
                        {
                            // I could not get this to work using gfx.Rotate/Translate Methods...
                            const double deg2Rad = 0.01745329251994329576923690768489;  // PI/180
                            var sr = Math.Sin(widget.Rotation * deg2Rad);
                            var cr = Math.Cos(widget.Rotation * deg2Rad);
                            // see PdfReference 1.7, Chapter 8.3.3 (Common Transformations)
                            // TODO: Is this always correct ? I had only the chance to test this with a 90 degree rotation...
                            form.PdfForm.Elements.SetMatrix(PdfFormXObject.Keys.Matrix, new XMatrix(cr, sr, -sr, cr, xRect.Width, 0));
                            if (widget.Rotation == 90 || widget.Rotation == 270)
                                xRect = new XRect(0, 0, rect.Height, rect.Width);
                        }
                        gfx.IntersectClip(xRect);       // TODO: Not sure, if we should clip AFTER drawing background and border... TESTME !
                        // fill background
                        if (!widget.BackColor.IsEmpty)
                        {
                            gfx.DrawRectangle(new XSolidBrush(widget.BackColor), xRect);
                        }
                        // draw border
                        if (widget.BorderWidth > 0 && !widget.BorderColor.IsEmpty)
                        {
                            gfx.DrawRectangle(new XPen(widget.BorderColor, widget.BorderWidth), xRect);
                        }
                        // for Multiline fields, we use XTextFormatter to handle line-breaks and a fixed TextFormat (only TopLeft is supported)
                        if (MultiLine)
                        {
                            var tf = new XTextFormatter(gfx);
                            tf.DrawString(text, Font, new XSolidBrush(ForeColor), xRect, XStringFormats.TopLeft);
                        }
                        else
                        {
                            if (Combined && MaxLength > 0)
                            {
                                var combWidth = xRect.Width / MaxLength;
                                var x = xRect.X;
                                var cw = combWidth;
                                var tb = new XSolidBrush(ForeColor);
                                for (var ti = 0; ti < text.Length; ti++)
                                {
                                    var combRect = new XRect(x, xRect.Y, cw, xRect.Height);
                                    gfx.DrawString(text[ti].ToString(), Font, tb, combRect, XStringFormats.Center);
                                    x += cw;
                                }
                            }
                            else
                                gfx.DrawString(text, Font, new XSolidBrush(ForeColor), xRect, format);
                        }
                    }
                }

                form.DrawingFinished();
                form.PdfForm.Elements.Add("/FormType", new PdfLiteral("1"));

                // Get existing or create new appearance dictionary.
                var ap = widget.Elements[PdfAnnotation.Keys.AP] as PdfDictionary;
                if (ap == null)
                {
                    ap = new PdfDictionary(_document);
                    widget.Elements[PdfAnnotation.Keys.AP] = ap;
                }
                widget.Elements.SetName(PdfAnnotation.Keys.AS, "/N");   // set appearance state

                // Set XRef to normal state
                ap.Elements["/N"] = form.PdfForm.Reference;

                var xobj = form.PdfForm;
                var s = xobj.Stream.ToString();
                // Thank you Adobe: Without putting the content in 'EMC brackets'
                // the text is not rendered by PDF Reader 9 or higher.
                s = "/Tx BMC\n" + s + "\nEMC";
                xobj.Stream.Value = new RawEncoding().GetBytes(s);
            }
        }

        internal override void PrepareForSave()
        {
            base.PrepareForSave();
            RenderAppearance();
        }

        internal override void Flatten()
        {
            base.Flatten();

            var text = Text;
            if (MaxLength > 0)
                text = text.Substring(0, Math.Min(Text.Length, MaxLength));
            if (text.Length > 0)
            {
                //Debug.WriteLine(String.Format("Rendering Field {0} ({1}) -> {2}", FullyQualifiedName, ObjectID, text));

                for (var i = 0; i < Annotations.Elements.Count; i++)
                {
                    var widget = Annotations.Elements[i];
                    var rect = widget.Rectangle;
                    if (!rect.IsEmpty)
                    {
                        using (var gfx = XGraphics.FromPdfPage(widget.Page))
                        {
                            // Note: Page origin [0,0] is bottom left !
                            if (text.Length > 0)
                            {
                                var xRect = new XRect(rect.X1, widget.Page.Height.Point - rect.Y2, rect.Width, rect.Height);
                                if (widget.Rotation != 0 && (widget.Flags & PdfAnnotationFlags.NoRotate) == 0)
                                {
                                    gfx.RotateAtTransform(-widget.Rotation, xRect.TopLeft);
                                    if (widget.Rotation == 90 || widget.Rotation == 270)
                                        xRect = new XRect(rect.X1 - rect.Height, widget.Page.Height.Point - rect.Y2, xRect.Height, xRect.Width);
                                }
                                var format = TextAlign == TextAlignment.Left ? XStringFormats.CenterLeft : TextAlign == TextAlignment.Center ? XStringFormats.Center : XStringFormats.CenterRight;
                                gfx.IntersectClip(xRect);
                                // for Multiline fields, we use XTextFormatter to handle line-breaks and a fixed TextFormat (only TopLeft is supported)
                                if (MultiLine)
                                {
                                    var tf = new XTextFormatter(gfx);
                                    tf.DrawString(text, Font, new XSolidBrush(ForeColor), xRect, XStringFormats.TopLeft);
                                }
                                else
                                {
                                    if (Combined && MaxLength > 0)
                                    {
                                        var combWidth = xRect.Width / MaxLength;
                                        var x = xRect.X;
                                        var cw = combWidth;
                                        var tb = new XSolidBrush(ForeColor);
                                        for (var ti = 0; ti < text.Length; ti++)
                                        {
                                            var combRect = new XRect(x, xRect.Y, cw, xRect.Height);
                                            gfx.DrawString(text[ti].ToString(), Font, tb, combRect, XStringFormats.Center);
                                            x += cw;
                                        }
                                    }
                                    else
                                        gfx.DrawString(text, Font, new XSolidBrush(ForeColor), xRect, format);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Predefined keys of this dictionary. 
        /// The description comes from PDF 1.4 Reference.
        /// </summary>
        public new class Keys : PdfAcroField.Keys
        {
            /// <summary>
            /// (Optional; inheritable) The maximum length of the field’s text, in characters.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string MaxLen = "/MaxLen";

            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            internal static DictionaryMeta Meta
            {
                get { return _meta ?? (_meta = CreateMeta(typeof(Keys))); }
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
