using ResTB.Translation;
using ResTB.Translation.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.Map.Events
{
    public enum ErrorCodes
    {
        [LocalizedDescription(nameof(Resources.MapControl_Error_UndefinedError), typeof(Resources))]
        UndefinedError = 9999,

        // Layer specific errors
        [LocalizedDescription(nameof(Resources.MapControl_Error_CouldNotLoadLayer), typeof(Resources))]
        CouldNotLoadLayer = 100,
        [LocalizedDescription(nameof(Resources.MapControl_Error_CouldNotAddLayer), typeof(Resources))]
        CouldNotAddLayer = 101,
        [LocalizedDescription(nameof(Resources.MapControl_Error_CouldNotConnectDatabase), typeof(Resources))]
        CouldNotConnectDatabase = 102,
        [LocalizedDescription(nameof(Resources.MapControl_Error_FailedToRunSQLQuery), typeof(Resources))]
        FailedToRunSQLQuery = 103,
        [LocalizedDescription(nameof(Resources.MapControl_Error_MissingDBConfiguration), typeof(Resources))]
        MissingDBConfiguration = 104,
        [LocalizedDescription(nameof(Resources.MapControl_Error_GdalWarpError), typeof(Resources))]
        GdalWarpError = 105,

        // Editing errors
        [LocalizedDescription(nameof(Resources.MapControl_Error_EditingNotAllowed), typeof(Resources))]
        EditingNotAllowed = 200,
        [LocalizedDescription(nameof(Resources.MapControl_Error_EditingNotSupported), typeof(Resources))]
        EditingNotSupported = 201,
        [LocalizedDescription(nameof(Resources.MapControl_Error_UseStartEditing), typeof(Resources))]
        UseStartEditing = 202,
        [LocalizedDescription(nameof(Resources.MapControl_Error_ShapeInvalid), typeof(Resources))]
        ShapeInvalid = 203,

        // Import / Export errors
        [LocalizedDescription(nameof(Resources.MapControl_Error_ImportExportError), typeof(Resources))]
        ImportExportError = 300
    }
    public class MapControl_Error : EventArgs
    {
        public string Error
        {
            get
            {
                // see https://social.msdn.microsoft.com/Forums/vstudio/en-US/91901e5d-6965-466e-9cbf-bb8003e2d471/error-handling-with-enumerations?forum=csharpgeneral
                string description = String.Empty;
                DescriptionAttribute da;

                FieldInfo fi = ErrorCode.GetType().
                            GetField(ErrorCode.ToString());
                da = (DescriptionAttribute)Attribute.GetCustomAttribute(fi,
                            typeof(DescriptionAttribute));
                if (da != null)
                    description = da.Description;
                else
                    description = ErrorCode.ToString();

                return description;
            }
            
        }
        public ErrorCodes ErrorCode { get; set; }
        public Exception InnerException { get; set; }
        public string InMethod { get; set; }
        public string AxMapError { get; set; }
    }
}
