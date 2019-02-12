using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// The ActorPattern classes encapsulate operations that are not specific 
/// to a particular data type.  An Actor class can except (n) parameters
/// passing to a method (function) or action (procedure) being executed 
/// and returning a State object in the Completed delegate.
/// 
/// The class Actor runs an action, it has no parameters.
///     
/// The class Actor<Z> executes a method which has no parameters, and an
/// action which has one parameter.
///     
/// The class Actor<A, Z> executes a method which has one parameter A
/// and returns type Z, and runs an action which has two parameters
/// A and Z.
/// 
/// The class Actor<A, B, Z> executes a function which which has two 
/// parameter; A and B and returns type Z, and runs an action which has 
/// three parameters A, B and Z.
///     
/// The class Actor<A, B, C, Z> executes a function which has three 
/// parameters; A, B and C and returns type Z, and runs an action  
/// which has four parameters A, B, C and Z.
///     
/// and so on...
///     
/// </summary>
namespace ActorPattern
{
    public interface IState
    {
        Exception Error { get; set; }
    }

    public class Delegator
    {
        public delegate void Completed(object sender, IState e);
    }

    ///// <summary>
    ///// Execute a long running action which has no parameters.
    ///// </summary>
    //public class Actor
    //{
    //    /// <summary>
    //    /// Execute a long running procedure which has no parameters.  The calling 
    //    /// application remains responsive, and receives a State object in the 
    //    /// WhenComplete delegate.  Check State.Error to see if the procedure completed 
    //    /// or encountered an exception.
    //    /// </summary>
    //    public virtual async Task Action(Action action, Delegator.Completed done)
    //    {
    //        var state = new State();

    //        await Task.Run(() =>
    //        {
    //            state.Run(action);

    //        }).ConfigureAwait(false);

    //        done?.Invoke(this, state);
    //    }

    //    public class State : IState
    //    {
    //        /// <summary>
    //        /// Capture any errors and send back to the calling thread.
    //        /// </summary>
    //        public Exception Error { get; set; }

    //        internal void Run(Action action)
    //        {
    //            try
    //            {
    //                action();
    //            }
    //            catch (Exception ex)
    //            {
    //                Error = ex;
    //            }
    //        }
    //    }
    //}

    /// <summary>
    /// Execute methods which have no parameters and returns type Z, and 
    /// actions which have one parameter defined as type Z.
    /// </summary>
    public class Callback<Z>
    {
        /// <summary>
        /// Queue the method Func<Z> in a thread pool and send the result 
        /// via the Completed delegate when finished.
        /// </summary>
        /// <param name="job">Method to run</param>
        /// <param name="done">Delegate when finished</param>
        public virtual void ThreadCallback(Func<Z> method, Delegator.Completed done)
        {
            // ThreadPool.QueueUserWorkItem takes an object which represents the data
            // to be used by the queued method in WaitCallback.  I'm using an anonymous 
            // delegate as the method in WaitCallback, and passing the variable state 
            // as the data to use.
            ThreadPool.QueueUserWorkItem(new WaitCallback((x) =>
            {
                var state = x as State;

                state.Run(method);

                // If the calling application neglected to provide a WhenComplete delegate
                // check if null before attempting to invoke.
                done?.Invoke(this, state);

            }), new State());
        }

        /// <summary>
        /// Execute the method Func<Z> in an awaitable task and send the result
        /// via the Completed delegate when finished.
        /// </summary>
        /// <param name="job">Method to run</param>
        /// <param name="done">Delegate when finished</param>
        /// <returns>Awaitable task</returns>
        public virtual async Task AsyncDelegate(Func<Z> method, Delegator.Completed done)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(method);

            }).ConfigureAwait(false);

            done?.Invoke(this, state);
        }

        public virtual async Task<IState> AsyncState(Func<Z> method)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(method);

            }).ConfigureAwait(false);

            return state;
        }

        public virtual async Task<Z> AsyncResult(Func<Z> method)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(method);

            }).ConfigureAwait(false);

            return state.Result;
        }

        /// <summary>
        /// Run a procedure which takes one parameter defined a Z, and invoke the
        /// Completed delegate when finished.
        /// </summary>
        /// <param name="action">Procedure to run</param>
        /// <param name="a">Paramter</param>
        /// <param name="done">Delegate when finished</param>
        /// <returns>Awaitable task</returns>
        public virtual async Task Action(Action<Z> action, Z param, Delegator.Completed done)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(action, param);

            }).ConfigureAwait(false);

            done?.Invoke(this, state);
        }

        /// <summary>
        /// Every actor class has it's own state so it understands what 
        /// each parameter type is.
        /// </summary>
        public class State : IState
        {
            /// <summary>
            /// This will be the response and will be sent back to the 
            /// calling thread using the delegate.
            /// </summary>
            public Z Result { get; private set; }

            /// <summary>
            /// Capture any errors and send back to the calling thread.
            /// </summary>
            public Exception Error { get; set; }

            /// <summary>
            ///  Set as an internal void so only the Actor class can  
            ///  invoke this method.
            /// </summary>
            internal void Run(Func<Z> method)
            {
                try
                {
                    Result = method();
                }
                catch (Exception ex)
                {
                    Error = ex;

                    Result = default(Z);
                }
            }

            /// <summary>
            /// Run the action using parameter Z.
            /// </summary>
            /// <param name="action">Action to run</param>
            /// <param name="param">Parameter Z</param>
            internal void Run(Action<Z> action, Z param)
            {
                try
                {
                    action(param);
                }
                catch (Exception ex)
                {
                    Error = ex;
                }
            }
        }
    }

    /// <summary>
    /// Execute methods which have one parameter and returns type Z, and 
    /// actions which have two parameters defined as type A and Z.
    /// </summary>
    public class Callback<A, Z>
    {
        public virtual void ThreadCallback(Func<A, Z> method, A param, Delegator.Completed done)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((x) =>
            {
                var state = x as State;

                state.Run(method, param);

                done?.Invoke(this, state);

            }), new State());
        }

        public virtual async Task AsyncDelegate(Func<A, Z> method, A param, Delegator.Completed done)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(method, param);

            }).ConfigureAwait(false);

            done?.Invoke(this, state);
        }

        public virtual async Task<IState> AsyncState(Func<A, Z> method, A param)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(method, param);

            }).ConfigureAwait(false);

            return state;
        }

        public virtual async Task<Z> AsyncResult(Func<A, Z> method, A param)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(method, param);

            }).ConfigureAwait(false);

            return state.Result;
        }

        public virtual async Task Action(Action<A, Z> action, A param, Z param2, Delegator.Completed done)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(action, param, param2);

            }).ConfigureAwait(false);

            done?.Invoke(this, state);
        }

        public class State : IState
        {
            public Z Result { get; private set; }

            public Exception Error { get; set; }

            internal void Run(Func<A, Z> method, A param)
            {
                try
                {
                    Result = method(param);
                }
                catch (Exception ex)
                {
                    Error = ex;

                    Result = default(Z);
                }
            }

            internal void Run(Action<A, Z> action, A param, Z param2)
            {
                try
                {
                    action(param, param2);
                }
                catch (Exception ex)
                {
                    Error = ex;
                }
            }
        }
    }

    /// <summary>
    /// Execute methods which have two parameters and returns type Z, and 
    /// actions which have three parameters defined as type A, B and Z.
    /// </summary>
    public class Callback<A, B, Z>
    {
        public virtual void ThreadCallback(Func<A, B, Z> method, A param, B param2, Delegator.Completed done)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((x) =>
            {
                var state = x as State;

                state.Run(method, param, param2);

                done?.Invoke(this, state);

            }), new State());
        }

        public virtual async Task AsyncDelegate(Func<A, B, Z> method, A param, B param2, Delegator.Completed done)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(method, param, param2);

            }).ConfigureAwait(false);

            done?.Invoke(this, state);
        }

        public virtual async Task<IState> AsyncState(Func<A, B, Z> method, A param, B param2)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(method, param, param2);

            }).ConfigureAwait(false);

            return state;
        }

        public virtual async Task<Z> AsyncResult(Func<A, B, Z> method, A param, B param2)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(method, param, param2);

            }).ConfigureAwait(false);

            return state.Result;
        }

        public virtual async Task Action(Action<A, B, Z> action, A param, B param2, Z param3, Delegator.Completed done)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(action, param, param2, param3);

            }).ConfigureAwait(false);

            done?.Invoke(this, state);
        }

        public class State : IState
        {
            public Z Result { get; private set; }

            public Exception Error { get; set; }

            internal void Run(Func<A, B, Z> method, A param, B param2)
            {
                try
                {
                    Result = method(param, param2);
                }
                catch (Exception ex)
                {
                    Error = ex;

                    Result = default(Z);
                }
            }

            internal void Run(Action<A, B, Z> action, A param, B param2, Z param3)
            {
                try
                {
                    action(param, param2, param3);
                }
                catch (Exception ex)
                {
                    Error = ex;
                }
            }
        }
    }

    /// <summary>
    /// Execute methods which have three parameters and returns type Z, and 
    /// actions which have four parameters defined as type A, B, C and Z.
    /// </summary>
    public class Callback<A, B, C, Z>
    {
        public virtual void ThreadCallback(Func<A, B, C, Z> method, A param, B param2, C param3, Delegator.Completed done)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((x) =>
            {
                var state = x as State;

                state.Run(method, param, param2, param3);

                done?.Invoke(this, state);

            }), new State());
        }

        public virtual async Task AsyncDelegate(Func<A, B, C, Z> method, A param, B param2, C param3, Delegator.Completed done)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(method, param, param2, param3);

            }).ConfigureAwait(false);

            done?.Invoke(this, state);
        }

        public virtual async Task<IState> AsyncState(Func<A, B, C, Z> method, A param, B param2, C param3)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(method, param, param2, param3);

            }).ConfigureAwait(false);

            return state;
        }

        public virtual async Task<Z> AsyncResult(Func<A, B, C, Z> method, A param, B param2, C param3)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(method, param, param2, param3);

            }).ConfigureAwait(false);

            return state.Result;
        }

        public virtual async Task Action(Action<A, B, C, Z> action, A param, B param2, C param3, Z param4, Delegator.Completed done)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(action, param, param2, param3, param4);

            }).ConfigureAwait(false);

            done?.Invoke(this, state);
        }

        public class State : IState
        {
            public Z Result { get; private set; }

            public Exception Error { get; set; }

            internal void Run(Func<A, B, C, Z> method, A param, B param2, C param3)
            {
                try
                {
                    Result = method(param, param2, param3);
                }
                catch (Exception ex)
                {
                    Error = ex;

                    Result = default(Z);
                }
            }

            internal void Run(Action<A, B, C, Z> action, A param, B param2, C param3, Z param4)
            {
                try
                {
                    action(param, param2, param3, param4);
                }
                catch (Exception ex)
                {
                    Error = ex;
                }
            }
        }
    }

    /// <summary>
    /// Execute methods which have four parameters and returns type Z, and 
    /// actions which have five parameters defined as type A, B, C, D and Z.
    /// </summary>
    public class Callback<A, B, C, D, Z>
    {
        public virtual void ThreadCallback(Func<A, B, C, D, Z> method, A param, B param2, C param3, D param4, Delegator.Completed done)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((x) =>
            {
                var state = x as State;

                state.Run(method, param, param2, param3, param4);

                done?.Invoke(this, state);

            }), new State());
        }

        public virtual async Task AsyncDelegate(Func<A, B, C, D, Z> method, A param, B param2, C param3, D param4, Delegator.Completed done)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(method, param, param2, param3, param4);

            }).ConfigureAwait(false);

            done?.Invoke(this, state);
        }

        public virtual async Task<IState> AsyncState(Func<A, B, C, D, Z> method, A param, B param2, C param3, D param4)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(method, param, param2, param3, param4);

            }).ConfigureAwait(false);

            return state;
        }

        public virtual async Task<Z> AsyncResult(Func<A, B, C, D, Z> method, A param, B param2, C param3, D param4)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(method, param, param2, param3, param4);

            }).ConfigureAwait(false);

            return state.Result;
        }

        public virtual async Task Action(Action<A, B, C, D, Z> action, A param, B param2, C param3, D param4, Z param5, Delegator.Completed done)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(action, param, param2, param3, param4, param5);

            }).ConfigureAwait(false);

            done?.Invoke(this, state);
        }

        public class State : IState
        {
            public Z Result { get; private set; }

            public Exception Error { get; set; }

            internal void Run(Func<A, B, C, D, Z> method, A param, B param2, C param3, D param4)
            {
                try
                {
                    Result = method(param, param2, param3, param4);
                }
                catch (Exception ex)
                {
                    Error = ex;

                    Result = default(Z);
                }
            }

            internal void Run(Action<A, B, C, D, Z> action, A param, B param2, C param3, D param4, Z param5)
            {
                try
                {
                    action(param, param2, param3, param4, param5);
                }
                catch (Exception ex)
                {
                    Error = ex;
                }
            }
        }
    }

    /// <summary>
    /// Execute methods which have five parameters and returns type Z, and 
    /// actions which have six parameters defined as type A, B, C, D and Z.
    /// </summary>
    public class Callback<A, B, C, D, E, Z>
    {
        public virtual void ThreadCallback(Func<A, B, C, D, E, Z> method, A param, B param2, C param3, D param4, E param5, Delegator.Completed done)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((x) =>
            {
                var state = x as State;

                state.Run(method, param, param2, param3, param4, param5);

                done?.Invoke(this, state);

            }), new State());
        }

        public virtual async Task AsyncDelegate(Func<A, B, C, D, E, Z> method, A param, B param2, C param3, D param4, E param5, Delegator.Completed done)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(method, param, param2, param3, param4, param5);

            }).ConfigureAwait(false);

            done?.Invoke(this, state);
        }

        public virtual async Task<IState> AsyncState(Func<A, B, C, D, E, Z> method, A param, B param2, C param3, D param4, E param5)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(method, param, param2, param3, param4, param5);

            }).ConfigureAwait(false);

            return state;
        }

        public virtual async Task<Z> AsyncResult(Func<A, B, C, D, E, Z> method, A param, B param2, C param3, D param4, E param5)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(method, param, param2, param3, param4, param5);

            }).ConfigureAwait(false);

            return state.Result;
        }

        public virtual async Task Action(Action<A, B, C, D, E, Z> action, A param, B param2, C param3, D param4, E param5, Z param6, Delegator.Completed done)
        {
            var state = new State();

            await Task.Run(() =>
            {
                state.Run(action, param, param2, param3, param4, param5, param6);

            }).ConfigureAwait(false);

            done?.Invoke(this, state);
        }

        public class State : IState
        {
            public Z Result { get; private set; }

            public Exception Error { get; set; }

            internal void Run(Func<A, B, C, D, E, Z> method, A param, B param2, C param3, D param4, E param5)
            {
                try
                {
                    Result = method(param, param2, param3, param4, param5);
                }
                catch (Exception ex)
                {
                    Error = ex;

                    Result = default(Z);
                }
            }

            internal void Run(Action<A, B, C, D, E, Z> action, A param, B param2, C param3, D param4, E param5, Z param6)
            {
                try
                {
                    action(param, param2, param3, param4, param5, param6);
                }
                catch (Exception ex)
                {
                    Error = ex;
                }
            }
        }
    }
}
