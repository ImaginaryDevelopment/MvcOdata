namespace Webby
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Description;
	using System.Threading;

	public abstract class ExceptionHandlingProxyBase<T> : ICommunicationObject, IDisposable
		where T : class
	{
		// state
		private bool IsOpened { get; set; }

		public bool IsDisposed { get; private set; }

		// lock
		private readonly object _channelLock = new object();

		private bool _isInitialized;

		private bool _isProxyCreated;

		private readonly ManualResetEvent _proxyRecreationLock = new ManualResetEvent(true);

		private int _proxyRecreationLockWait = 1000;

		protected int ProxyRecreationLockWait
		{
			get
			{
				return this._proxyRecreationLockWait;
			}

			set
			{
				this._proxyRecreationLockWait = value;
			}
		}

		// channel
		protected ChannelFactory<T> ChannelFactory { get; set; }

		private T _channel;

		#region Constructors

		protected ExceptionHandlingProxyBase()
		{
			this.ChannelFactory = null;
		}

		protected ExceptionHandlingProxyBase(string endpointConfigurationName)
		{
			this.ChannelFactory = null;
			this.Initialize(endpointConfigurationName);
		}

		protected virtual void Initialize(string endpointConfigurationName)
		{
			if (this._isInitialized)
			{
				throw new InvalidOperationException("Object already initialized.");
			}

			this._isInitialized = true;

			this.ChannelFactory = new ChannelFactory<T>(endpointConfigurationName);
		}

		protected ExceptionHandlingProxyBase(string endpointConfigurationName, string remoteAddress)
		{
			this.ChannelFactory = null;
			this.Initialize(endpointConfigurationName, new EndpointAddress(remoteAddress));
		}

		protected ExceptionHandlingProxyBase(string endpointConfigurationName, EndpointAddress remoteAddress)
		{
			this.ChannelFactory = null;
			this.Initialize(endpointConfigurationName, remoteAddress);
		}

		protected virtual void Initialize(string endpointConfigurationName, EndpointAddress remoteAddress)
		{
			if (this._isInitialized)
			{
				throw new InvalidOperationException("Object already initialized.");
			}

			this._isInitialized = true;

			this.ChannelFactory = new ChannelFactory<T>(endpointConfigurationName, remoteAddress);
		}

		protected ExceptionHandlingProxyBase(Binding binding, EndpointAddress remoteAddress)
		{
			this.ChannelFactory = null;
			this.Initialize(binding, remoteAddress);
		}

		protected virtual void Initialize(Binding binding, EndpointAddress remoteAddress)
		{
			if (this._isInitialized)
			{
				throw new InvalidOperationException("Object already initialized.");
			}

			this._isInitialized = true;

			this.ChannelFactory = new ChannelFactory<T>(binding, remoteAddress);
		}

		#endregion

		#region Proxy creation

		public event EventHandler AfterRecreateProxy;

		protected virtual void CreateProxy()
		{
			lock (this._channelLock)
			{
				if (this._isProxyCreated)
				{
					throw new InvalidOperationException("Proxy already created.");
				}

				this.CreateInnerChannel();
				this._isProxyCreated = true;
			}
		}

		protected virtual void RecreateProxy()
		{
			lock (this._channelLock)
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				this.CreateInnerChannel();

				if (this.AfterRecreateProxy != null)
				{
					this.AfterRecreateProxy(this, null);
				}
			}
		}

		private void CreateInnerChannel()
		{
			lock (this._channelLock)
			{
				if (this.ChannelFactory == null)
				{
					throw new InvalidOperationException("Proxy invalid. This occurs when you use the default constructor.");
				}

				this._channel = this.ChannelFactory.CreateChannel();

				var channelObject = this._channel as ICommunicationObject;
				if (channelObject == null)
				{
					return;
				}

				channelObject.Faulted += this.InnerChannelFaulted;
				channelObject.Closed += this.InnerChannelClosed;
				channelObject.Closing += this.InnerChannelClosing;
				channelObject.Opened += this.InnerChannelOpened;
				channelObject.Opening += this.InnerChannelOpening;
			}
		}

		#endregion

		#region Communication events

		private void InnerChannelOpening(object sender, EventArgs e)
		{
			lock (this._channelLock)
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				if (this.Opening != null)
				{
					this.Opening(sender, e);
				}
			}
		}

		private void InnerChannelOpened(object sender, EventArgs e)
		{
			lock (this._channelLock)
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				if (this.Opened != null)
				{
					this.Opened(sender, e);
				}
			}
		}

		private void InnerChannelClosing(object sender, EventArgs e)
		{
			lock (this._channelLock)
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				if (this.Closing != null)
				{
					this.Closing(sender, e);
				}
			}
		}

		private void InnerChannelClosed(object sender, EventArgs e)
		{
			lock (this._channelLock)
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				try
				{
					this._proxyRecreationLock.Reset(); // will stop other threads from trying to Invoke() while recreating the proxy

					if (this.Closed != null)
					{
						this.Closed(sender, e);
					}

					this.OnClosed();
				}
				finally
				{
					this._proxyRecreationLock.Set(); // will stop other threads from trying to Invoke() while recreating the proxy
				}
			}
		}

		protected virtual void OnClosed()
		{
			lock (this._channelLock)
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				this.Abort();
				this.RecreateProxy();
			}
		}

		private void InnerChannelFaulted(object sender, EventArgs e)
		{
			lock (this._channelLock)
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				try
				{
					this._proxyRecreationLock.Reset(); // will stop other threads from trying to Invoke() while recreating the proxy

					if (this.Faulted != null)
					{
						this.Faulted(sender, e);
					}

					this.OnFaulted();
				}
				finally
				{
					this._proxyRecreationLock.Set(); // will stop other threads from trying to Invoke() while recreating the proxy
				}
			}
		}

		protected virtual void OnFaulted()
		{
			lock (this._channelLock)
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				this.Abort();
				this.RecreateProxy();
			}
		}

		#endregion

		#region Channel Properties

		public T Channel
		{
			get
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				return this._channel;
			}
		}

		public IClientChannel InnerChannel
		{
			get
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				return (IClientChannel)this._channel;
			}
		}

		public ClientCredentials ClientCredentials
		{
			get
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				return this.ChannelFactory.Credentials;
			}
		}

		public ServiceEndpoint Endpoint
		{
			get
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				return this.ChannelFactory.Endpoint;
			}
		}

		public CommunicationState State
		{
			get
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				var channel = (IChannel)this._channel;
				if (channel == null)
				{
					return CommunicationState.Created;
				}

				return channel.State;
			}
		}

		#endregion

		#region ICommunicationObject Members

		public event EventHandler Closed;

		public event EventHandler Closing;

		public event EventHandler Faulted;

		public event EventHandler Opened;

		public event EventHandler Opening;

		public void Abort()
		{
			lock (this._channelLock)
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				var co = (ICommunicationObject)this._channel;
				co.Closed -= this.InnerChannelClosed;
				co.Closing -= this.InnerChannelClosing;
				co.Faulted -= this.InnerChannelFaulted;
				co.Opened -= this.InnerChannelOpened;
				co.Opening -= this.InnerChannelOpening;
				co.Abort();
			}
		}

		public void Open(TimeSpan timeout)
		{
			lock (this._channelLock)
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				if (!this.IsOpened)
				{
					this.EnsureProxy();
					((ICommunicationObject)this._channel).Open(timeout);
					this.IsOpened = true;
				}
			}
		}

		public void Open()
		{
			lock (this._channelLock)
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				if (!this.IsOpened)
				{
					this.EnsureProxy();
					((ICommunicationObject)this._channel).Open();
					this.IsOpened = true;
				}
			}
		}

		public void Close(TimeSpan timeout)
		{
			lock (this._channelLock)
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				((ICommunicationObject)this._channel).Close(timeout);
			}
		}

		public void Close()
		{
			lock (this._channelLock)
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				((ICommunicationObject)this._channel).Close();
			}
		}

		public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
		{
			lock (this._channelLock)
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				return ((ICommunicationObject)this._channel).BeginClose(timeout, callback, state);
			}
		}

		public IAsyncResult BeginClose(AsyncCallback callback, object state)
		{
			lock (this._channelLock)
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				return ((ICommunicationObject)this._channel).BeginClose(callback, state);
			}
		}

		public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
		{
			lock (this._channelLock)
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				return ((ICommunicationObject)this._channel).BeginClose(timeout, callback, state);
			}
		}

		public IAsyncResult BeginOpen(AsyncCallback callback, object state)
		{
			lock (this._channelLock)
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				return ((ICommunicationObject)this._channel).BeginOpen(callback, state);
			}
		}

		public void EndClose(IAsyncResult result)
		{
			lock (this._channelLock)
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				((ICommunicationObject)this._channel).EndClose(result);
			}
		}

		public void EndOpen(IAsyncResult result)
		{
			lock (this._channelLock)
			{
				if (this.IsDisposed)
				{
					throw new InvalidOperationException("Cannot use disposed object.");
				}

				((ICommunicationObject)this._channel).EndOpen(result);
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			lock (this._channelLock)
			{
				this.Cleanup();
				this.IsDisposed = true;
			}
		}

		protected virtual void Cleanup()
		{
			try
			{
				var co = (ICommunicationObject)this._channel;
				co.Closed -= this.InnerChannelClosed;
				co.Closing -= this.InnerChannelClosing;
				co.Faulted -= this.InnerChannelFaulted;
				co.Opened -= this.InnerChannelOpened;
				co.Opening -= this.InnerChannelOpening;
				co.Close();
			}
			catch
			{
				try
				{
					var co = (ICommunicationObject)this._channel;
					co.Abort();
				}
					// ReSharper disable EmptyGeneralCatchClause
				catch
					// ReSharper restore EmptyGeneralCatchClause
				{
				}
			}

			try
			{
				this.ChannelFactory.Close();
			}
			catch
			{
				try
				{
					this.ChannelFactory.Abort();
				}
					// ReSharper disable EmptyGeneralCatchClause
				catch
					// ReSharper restore EmptyGeneralCatchClause
				{
				}
			}
		}

		#endregion

		#region Invoke

		public delegate void RetryInvokeHandler(out Message unreadMessage);

		public event RetryInvokeHandler RetryInvoke;

		protected void Invoke(string operationName, params object[] parameters)
		{
			if (this.IsDisposed)
			{
				throw new InvalidOperationException("Cannot use disposed object.");
			}

			this.Open();

			var methodInfo = this.GetMethod(operationName);

			try
			{
				// manual reset event here, turn it on when faulted
				// other threads will queue, and get a successful Invoke() once proxy is recreated
				this._proxyRecreationLock.WaitOne(this.ProxyRecreationLockWait); // if this takes longer than 1 second we have bigger problems
				methodInfo.Invoke(this._channel, parameters);
			}
			catch (TargetInvocationException targetEx)
			{
				// Invoke() always throws this type
				var commEx = targetEx.InnerException as CommunicationException;
				if (commEx == null)
				{
					throw targetEx.InnerException; // not a communication exception, throw it
				}

				var faultEx = commEx as FaultException;
				if (faultEx != null)
				{
					throw targetEx.InnerException; // the service threw a fault, throw it
				}

				try
				{
					// manual reset event here, turn it on when faulted
					// other threads will queue, and get a successful Invoke() once proxy is recreated
					this._proxyRecreationLock.WaitOne(this.ProxyRecreationLockWait); // if this takes longer than 1 second we have bigger problems

					// if it is a Message type it won't work, must fire RetryInvoke() and hopefully derived class will supply the original
					// message to send again...
					if (parameters.Length == 1 && parameters[0] is Message)
					{
						Message unreadMessage;
						this.RetryInvoke(out unreadMessage);
						methodInfo.Invoke(this._channel, new object[] { unreadMessage }); // a communication exception, retry once
					}
					else
					{
						methodInfo.Invoke(this._channel, parameters); // a communication exception, retry once
					}
				}
				catch (TargetInvocationException targetEx2)
				{
					throw targetEx2.InnerException; // still failed, throw it
				}
			}
		}

		protected TResult Invoke<TResult>(string operationName, params object[] parameters)
		{
			var methodInfo = this.GetMethod(operationName);
			return this.Invoke<TResult>(methodInfo, parameters);
		}

		protected TResult Invoke<TResult>(MethodInfo methodInfo, params object[] parameters)
		{
			if (this.IsDisposed)
			{
				throw new InvalidOperationException("Cannot use disposed object.");
			}

			this.Open();

			TResult result;

			try
			{
				// manual reset event here, turn it on when faulted
				// other threads will queue, and get a successful Invoke() once proxy is recreated
				this._proxyRecreationLock.WaitOne(this.ProxyRecreationLockWait); // if this takes longer than 1 second we have bigger problems
				result = (TResult)methodInfo.Invoke(this._channel, parameters);
			}
			catch (TargetInvocationException targetEx)
			{
				// Invoke() always throws this type
				var commEx = targetEx.InnerException as CommunicationException;
				if (commEx == null)
				{
					throw targetEx.InnerException; // not a communication exception, throw it
				}

				var faultEx = commEx as FaultException;
				if (faultEx != null)
				{
					throw targetEx.InnerException; // the service threw a fault, throw it
				}

				// a communication exception, retry once
				try
				{
					// manual reset event here, turn it on when faulted
					// other threads will queue, and get a successful Invoke() once proxy is recreated
					this._proxyRecreationLock.WaitOne(this.ProxyRecreationLockWait); // if this takes longer than 1 second we have bigger problems

					// if it is a Message type it won't work, must fire RetryInvoke() and hopefully derived class will supply the original
					// message to send again...
					if (parameters.Length == 1 && parameters[0] is Message)
					{
						Message unreadMessage;
						this.RetryInvoke(out unreadMessage);
						result = (TResult)methodInfo.Invoke(this._channel, new object[] { unreadMessage }); // communication exception, retry once
					}
					else
					{
						result = (TResult)methodInfo.Invoke(this._channel, parameters); // communication exception, retry once
					}
				}
				catch (TargetInvocationException targetEx2)
				{
					throw targetEx2.InnerException; // still failed, throw it
				}
			}

			return result;
		}

		internal MethodInfo GetMethod(string operationName)
		{
			var t = typeof(T);

			var methods = new HashSet<MethodInfo>();
			this.GetMethodsRecursive(t, BindingFlags.Public | BindingFlags.Instance, ref methods);

			var result = (from m in methods where m.Name == operationName select m).ToArray();

			if (!result.Any())
			{
				throw new InvalidOperationException(string.Format("Unable to invoke method {0}. Method does not exist on contract {1}.", operationName, t));
			}

			if (result.Count() > 1)
			{
				throw new InvalidOperationException(
					string.Format(
						"Unable to invoke method {0}. More than one method is defined on contract {1} by the same name. Overloads not supported by CachedProxyBase.",
						operationName,
						t));
			}

			return result.First();
		}

		private void GetMethodsRecursive(Type t, BindingFlags flags, ref HashSet<MethodInfo> methods)
		{
			var children = t.GetMethods(flags);
			methods.UnionWith(children);
			foreach (var contract in t.GetInterfaces())
			{
				this.GetMethodsRecursive(contract, flags, ref methods);
			}
		}

		private void EnsureProxy()
		{
			lock (this._channelLock)
			{
				if (!this._isProxyCreated)
				{
					this.CreateProxy();
				}
			}
		}

		#endregion
	}
}