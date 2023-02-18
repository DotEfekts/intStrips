using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using intStrips.Models;
using intStripsShared.Models;

namespace intStrips.Services
{
    public class VatSysConnector : IDisposable
    {
        public static VatSysConnector Instance { get; }
        
        private readonly BinaryFormatter _formatter = new BinaryFormatter();
        private readonly Semaphore _clientSemaphore;
        private readonly Dispatcher _dispatcher;

        private bool _disposed;
        private NamedPipeServerStream _serverStream;
        private NamedPipeClientStream _clientStream;


        static VatSysConnector()
        {
            Instance = new VatSysConnector();
        }

        private VatSysConnector()
        {
            var serverThread = new Thread(StartPipeServer);
            serverThread.Start();  
            
            _clientSemaphore = new Semaphore(1, 1);
            _dispatcher = Application.Current.Dispatcher;
        }

        private void StartPipeServer()
        {
            while (!_disposed)
            {
                try
                {
                    _serverStream = new NamedPipeServerStream("intStripsClient", PipeDirection.InOut, 1, PipeTransmissionMode.Byte,PipeOptions.Asynchronous);
                    _serverStream.WaitForConnection();

                    while (_serverStream.IsConnected)
                    {
                        try
                        {
                            var message = (PipeMessageModel)_formatter.Deserialize(_serverStream);
                            Debug.WriteLine(message);


                            if (message.ModelType == ModelType.ADD_REQUEST)
                            {
                                var add = message as AddRequestModel;
                                _dispatcher.Invoke(() =>
                                {
                                    FlightDataAdded?.Invoke(this, add.Data);
                                });
                                
                                _formatter.Serialize(_serverStream, new RequestResponseModel
                                {
                                    RequestId = message.RequestId,
                                    Success = true
                                });
                                continue;
                            }

                            if (message.ModelType == ModelType.REMOVE_REQUEST)
                            {
                                var remove = message as RemoveRequestModel;
                                _dispatcher.Invoke(() =>
                                {
                                    FlightDataRemoved?.Invoke(this, remove.Callsign);
                                });
                                
                                _formatter.Serialize(_serverStream, new RequestResponseModel
                                {
                                    RequestId = message.RequestId,
                                    Success = true
                                });
                                continue;
                            }

                            if (message.ModelType == ModelType.UPDATE_REQUEST)
                            {
                                var update = message as UpdateRequestModel;
                                _dispatcher.Invoke(() =>
                                {
                                    FlightDataChanged?.Invoke(this, new FlightDataChangedArgs
                                    {
                                        Callsign = update.Callsign,
                                        Field = update.Field,
                                        Update = update.Value
                                    });
                                });
                                
                                _formatter.Serialize(_serverStream, new RequestResponseModel
                                {
                                    RequestId = message.RequestId,
                                    Success = true
                                });
                            }

                            if (message.ModelType == ModelType.CONTROL_INFO_CHANGED)
                            {
                                var control = message as ControlInfoChanged;
                                _dispatcher.Invoke(() =>
                                {
                                    ControlInfoChanged?.Invoke(this, control.Data);
                                });
                                
                                _formatter.Serialize(_serverStream, new RequestResponseModel
                                {
                                    RequestId = message.RequestId,
                                    Success = true
                                });
                            }
                        }
                        catch (SerializationException ex) 
                        {
                            Debug.WriteLine(ex.Message);
                            Debug.WriteLine(ex.StackTrace);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                }
                finally
                {
                    _serverStream?.Dispose();
                }
            }
        }

        private bool EnsureClientConnected()
        {
            if (!_disposed && (_clientStream == null || !_clientStream.IsConnected))
            {
                _clientStream = new NamedPipeClientStream(".", "intStripsServer", PipeDirection.InOut, PipeOptions.Asynchronous);
                try
                {
                    _clientStream.Connect(1000);
                }
                catch
                {
                    // ignored
                }
            }

            return _clientStream != null && _clientStream.IsConnected;
        }
        
        public event EventHandler<VatSysFlightDataModel[]> DataRefreshComplete;
        public event EventHandler<VatSysFlightDataModel> FlightDataAdded;
        public event EventHandler<FlightDataChangedArgs> FlightDataChanged;
        public event EventHandler<string> FlightDataRemoved;
        public event EventHandler<ControlInfoModel> ControlInfoChanged;

        public void SendUpdateRequest(string callsign, string field, string value)
        {
            var requestId = Guid.NewGuid().ToString();
            SendClientMessage(new UpdateRequestModel()
            {
                RequestId = requestId,
                Callsign = callsign,
                Field = field,
                Value = value
            });
        }

        public void SendFdrListRequest()
        {
            var requestId = Guid.NewGuid().ToString();
            var response = SendClientMessage(new CommandRequestModel()
            {
                RequestId = requestId,
                Command = CommandRequestModel.CommandType.FDR_REFRESH
            });
            
            if(response != null)
                _dispatcher.Invoke(() =>
                {
                    DataRefreshComplete?.Invoke(this, response.Data as VatSysFlightDataModel[]);
                });
        }

        public void SendControlInfoRequest()
        {
            var requestId = Guid.NewGuid().ToString();
            var response = SendClientMessage(new CommandRequestModel()
            {
                RequestId = requestId,
                Command = CommandRequestModel.CommandType.CONTROL_INFO_REFRESH
            });
            
            if(response != null)
                _dispatcher.Invoke(() =>
                {
                    ControlInfoChanged?.Invoke(this, response.Data as ControlInfoModel);
                });
        }

        public void SendOpenFlightPlanCommand(string callsign)
        {
            var requestId = Guid.NewGuid().ToString();
            SendClientMessage(new CommandRequestModel()
            {
                RequestId = requestId,
                Command = CommandRequestModel.CommandType.OPEN_FLIGHT_PLAN,
                Data = callsign
            });
        }

        private RequestResponseModel SendClientMessage(PipeMessageModel message)
        {
            if (!_clientSemaphore.WaitOne()) return null;
            try
            {
                if (!EnsureClientConnected())
                    return null;
                
                var tokenSource = new CancellationTokenSource();
                Task.Run(() => StartTimeout(tokenSource.Token), tokenSource.Token);
                
                _formatter.Serialize(_clientStream, message);
                var response = _formatter.Deserialize(_clientStream);
                
                tokenSource.Cancel();
                
                return response as RequestResponseModel;
            }
            finally
            {
                _clientSemaphore.Release();
            }
        }

        private void StartTimeout(CancellationToken token)
        {
            Thread.Sleep(1000);
            if(!token.IsCancellationRequested)
                _clientStream.Dispose();
        }
        
        public void Dispose()
        {
            _disposed = true;
            _serverStream?.Dispose();
            _clientStream?.Dispose();
        }

        public void RequestSsrAutoAssign(string callsign)
        {
            if (!_clientSemaphore.WaitOne()) return;
            if (!EnsureClientConnected())
            {
                _clientSemaphore.Release();
                return;
            }
            
            var requestId = Guid.NewGuid().ToString();

            _formatter.Serialize(_clientStream, new CommandRequestModel()
            {
                RequestId = requestId,
                Command = CommandRequestModel.CommandType.AUTO_ASSIGN_SSR,
                Data = callsign
            });
            
            // We don't use the response but we do need to flush it from the pipe
            _formatter.Deserialize(_clientStream);
            
            _clientSemaphore.Release();
        }
    }
}