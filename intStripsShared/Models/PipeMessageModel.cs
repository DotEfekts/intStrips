using System;

namespace intStripsShared.Models
{
    [Serializable]
    public abstract class PipeMessageModel
    {
        public string RequestId;
        public ModelType ModelType;
    }

    public enum ModelType
    {
        COMMAND_REQUEST, ADD_REQUEST, REMOVE_REQUEST, UPDATE_REQUEST, REQUEST_RESPONSE,
        CONTROL_INFO_CHANGED
    }

    [Serializable]
    public class AddRequestModel : PipeMessageModel
    {
        public AddRequestModel()
        {
            ModelType = ModelType.ADD_REQUEST;
        }
        
        public VatSysFlightDataModel Data;
    }

    [Serializable]
    public class RemoveRequestModel : PipeMessageModel
    {
        public RemoveRequestModel()
        {
            ModelType = ModelType.REMOVE_REQUEST;
        }
        
        public string Callsign;
    }
    
    [Serializable]
    public class CommandRequestModel : PipeMessageModel
    {
        public CommandRequestModel()
        {
            ModelType = ModelType.COMMAND_REQUEST;
        }
        
        public CommandType Command;
        public string Data;

        public enum CommandType
        {
            OPEN_FLIGHT_PLAN, 
            FDR_REFRESH,
            CONTROL_INFO_REFRESH,
            AUTO_ASSIGN_SSR
        }
    }
    
    [Serializable]
    public class UpdateRequestModel : PipeMessageModel
    {
        public UpdateRequestModel()
        {
            ModelType = ModelType.UPDATE_REQUEST;
        }
        
        public string Callsign;
        public string Field;
        public string Value;
    }
    
    [Serializable]
    public class RequestResponseModel : PipeMessageModel
    {
        public RequestResponseModel()
        {
            ModelType = ModelType.REQUEST_RESPONSE;
        }
        
        public bool Success;
        public object Data;
    }
    

    [Serializable]
    public class ControlInfoChanged : PipeMessageModel
    {
        public ControlInfoChanged()
        {
            ModelType = ModelType.CONTROL_INFO_CHANGED;
        }
        
        public ControlInfoModel Data;
    }
}