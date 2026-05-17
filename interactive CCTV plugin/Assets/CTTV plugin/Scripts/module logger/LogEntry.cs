using System;

namespace Surveillance.Logs
{
 
    public enum LogCategory
    {
        System,         
        EventTrigger,  
        RawDetection   
    }


    [Serializable]
    public struct LogEntry
    {
        public DateTime Timestamp;       
        public LogCategory Category;     
        public int SourceId;             
        public string Message;           
        public string Details;           

        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Category}] [Cam: {SourceId}] {Message} | {Details}";
        }
    }
}