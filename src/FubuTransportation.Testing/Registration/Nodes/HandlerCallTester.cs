using System;
using FubuCore;
using FubuCore.Reflection;
using FubuMVC.Core.Registration.Nodes;
using FubuTestingSupport;
using FubuTransportation.Registration.Nodes;
using FubuTransportation.Runtime;
using NUnit.Framework;

namespace FubuTransportation.Testing.Registration.Nodes
{
    [TestFixture]
    public class HandlerCallTester
    {
        // TODO -- need to do some end to end testing with these against the container

        [Test]
        public void choose_handler_type_for_one_in_one_out()
        {
            var handler = HandlerCall.For<ITargetHandler>(x => x.OneInOneOut(null));

            var objectDef = handler.As<IContainerModel>().ToObjectDef();

            objectDef.Type.ShouldEqual(typeof (CascadingHandlerInvoker<ITargetHandler, Input, Output>));
        }

        [Test]
        public void choose_handler_type_for_one_in_zero_out()
        {
            var handler = HandlerCall.For<ITargetHandler>(x => x.OneInZeroOut(null));

            var objectDef = handler.As<IContainerModel>().ToObjectDef();

            objectDef.Type.ShouldEqual(typeof(SimpleHandlerInvoker<ITargetHandler, Input>));
        }

        [Test]
        public void throws_chunks_if_you_try_to_use_a_method_with_no_inputs()
        {
            Exception<ArgumentOutOfRangeException>.ShouldBeThrownBy(() => {
                HandlerCall.For<ITargetHandler>(x => x.ZeroInZeroOut());
            });
        }

        [Test]
        public void is_candidate()
        {
            HandlerCall.IsCandidate(ReflectionHelper.GetMethod<ITargetHandler>(x => x.ZeroInZeroOut())).ShouldBeFalse();
            HandlerCall.IsCandidate(ReflectionHelper.GetMethod<ITargetHandler>(x => x.OneInOneOut(null))).ShouldBeTrue();
            HandlerCall.IsCandidate(ReflectionHelper.GetMethod<ITargetHandler>(x => x.OneInZeroOut(null))).ShouldBeTrue();
            HandlerCall.IsCandidate(ReflectionHelper.GetMethod<ITargetHandler>(x => x.ManyIn(null, null))).ShouldBeFalse();
        }

        [Test]
        public void could_handle()
        {
            var handler1 = HandlerCall.For<SomeHandler>(x => x.Interface(null));
            var handler2 = HandlerCall.For<SomeHandler>(x => x.BaseClass(null));
        
            handler1.CouldHandleOtherMessageType(typeof(Input1)).ShouldBeTrue();
            handler2.CouldHandleOtherMessageType(typeof(Input1)).ShouldBeTrue();
            
            handler1.CouldHandleOtherMessageType(typeof(Input2)).ShouldBeFalse();
            handler1.CouldHandleOtherMessageType(typeof(Input2)).ShouldBeFalse();


        }

        [Test]
        public void could_handle_is_false_for_its_own_input_type()
        {
            var handler = HandlerCall.For<ITargetHandler>(x => x.OneInOneOut(null));
            handler.CouldHandleOtherMessageType(typeof(Input)).ShouldBeFalse();
        }

        [Test]
        public void handler_equals()
        {
            var handler1 = HandlerCall.For<SomeHandler>(x => x.Interface(null));
            var handler2 = HandlerCall.For<SomeHandler>(x => x.Interface(null));
            var handler3 = HandlerCall.For<SomeHandler>(x => x.Interface(null));

            handler1.ShouldEqual(handler2);
            handler1.ShouldEqual(handler3);
            handler3.ShouldEqual(handler2);
            handler2.ShouldEqual(handler1);
        }
    }


}