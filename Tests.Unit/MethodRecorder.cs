﻿using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using Utilities.Reflection;

namespace Tests.Unit
{
	/// <summary>
	/// Proxy that records method invocations.
	/// </summary>
	public class MethodRecorder<T> : RealProxy
	{
		/// <summary>
		/// Creates a new interceptor that records method invocations.
		/// </summary>
		public MethodRecorder()
			: base(typeof(T))
		{
			_proxy = new Lazy<T>(() => (T)base.GetTransparentProxy());
		}

		/// <summary>
		/// The underlying proxy.
		/// </summary>
		public T Proxy
		{
			get { return _proxy.Value; }
		}

		/// <summary>
		/// The most recent invocation made on the proxy.
		/// </summary>
		public IMethodCallMessage LastInvocation { get; private set; }

		/// <see cref="RealProxy.Invoke"/>
		public override IMessage Invoke(IMessage msg)
		{
			var methodCall = msg as IMethodCallMessage;
			LastInvocation = methodCall;

			object returnValue = null;
			var method = methodCall.MethodBase as MethodInfo;
			if (method != null)
				returnValue = method.ReturnType.GetDefaultValue();

			return new ReturnMessage(returnValue, new object[0], 0, methodCall.LogicalCallContext, methodCall);
		}

		private readonly Lazy<T> _proxy;
	}
}