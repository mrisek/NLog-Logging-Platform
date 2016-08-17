using NLog;

namespace NLogPoC
{
    public class Logging
    {
        #region Member variables

        // create a logger instance with the same name of the class
        public static Logger logger = LogManager.GetCurrentClassLogger();

        // declare enumeration that consists of different NLog levels
        public enum Logs : byte { Trace = 1, Debug, Info, Warn, Error, Fatal };

        // it's also possible to control the Logger's name
        //Logger logger = LogManager.GetLogger("MyClassName")

        #endregion

        #region Logging

        /*
            Six types of log levels in NLog:

            1) Trace - very detailed logs, which may include high-volume information such as protocol payloads. This log level is typically only enabled during development
            2) Debug - debugging information, less detailed than trace, typically not enabled in production environment.
            3) Info - information messages, which are normally enabled in production environment
            4) Warn - warning messages, typically for non-critical issues, which can be recovered or which are temporary failures
            5) Error - error messages - most of the time these are Exceptions
            6) Fatal - very serious errors!
        */

        /// <summary>   
        /// Method for writing sample diagnostic messages at six different log levels
        /// Those are Trace, Debug, Info, Warn, Error and Fatal level
        /// </summary>
        public static void WriteLogMessages()
        {
            logger.Trace("Sample trace message");
            logger.Debug("Sample debug message");
            logger.Info("Sample informational message");
            logger.Warn("Sample warning message");
            logger.Error("Sample error message");
            logger.Fatal("Sample fatal error message");

            // alternatively you can call the Log() method and pass log level as the parameter
            logger.Log(LogLevel.Info, "Sample informational message");
        }

        /// <summary>
        /// Method for writing parameterizedLogMessages at six different log levels
        /// </summary>
        public static void WriteParameterizedLogMessagges()
        {
            // example of custom variables that are used for formating the string message
            int a = 23;
            int b = 54;

            // This is preferable way of formating string instead of String.Format() or concatenation due to NLog performace
            logger.Trace("Sample trace message, a={0}, b={1}", a, b);
            logger.Debug("Sample debug message, a={0}, b={1}", a, b);
            logger.Info("Sample informational message, a={0}, b={1}", a, b);
            logger.Trace("Sample warning message, a={0}, b={1}", a, b);
            logger.Trace("Sample error message, a={0}, b={1}", a, b);
            logger.Trace("Sample fatal errror message, a={0}, b={1}", a, b);
            logger.Log(LogLevel.Info, "Sample informational message, a={0}, b={1}", a, b);
        }

        /// <summary>
        /// Method for creating six possible NLog levels with custom text message
        /// </summary>
        /// <param name="logs">log level</param>
        /// <param name="message">custom text message</param>
        public static void LogMessage(Logs logs, string message)
        {
            switch (logs)
            {
                case Logs.Trace:
                    logger.Trace(message);
                    break;
                case Logs.Debug:
                    logger.Debug(message);
                    break;
                case Logs.Info:
                    logger.Info(message);
                    break;
                case Logs.Warn:
                    logger.Warn(message);
                    break;
                case Logs.Error:
                    logger.Error(message);
                    break;
                case Logs.Fatal:
                    logger.Fatal(message);
                    break;
            }
        }

        /// <summary>
        /// Method for creating six possible NLog levels with custom text message and parameter
        /// </summary>
        /// <param name="logs">log level</param>
        /// <param name="message">custom text message</param>
        /// <param name="param">extra parameter</param>
        public static void LogMessage(Logs logs, string message, string param)
        {
            switch (logs)
            {
                case Logs.Trace:
                    logger.Trace("{0} - {1}", message, param);
                    break;
                case Logs.Debug:
                    logger.Debug("{0} - {1}", message, param);
                    break;
                case Logs.Info:
                    logger.Info("{0} - {1}", message, param);
                    break;
                case Logs.Warn:
                    logger.Warn("{0} - {1}", message, param);
                    break;
                case Logs.Error:
                    logger.Error("{0} - {1}", message, param);
                    break;
                case Logs.Fatal:
                    logger.Fatal("{0} - {1}", message, param);
                    break;
            }
        }

        #endregion
    }
}
