/** @jsx React.DOM */

var React = FubuDiagnostics.React;
var Router = require('react-router');

var Handlers = require('./fubu-transportation-handlers');
var Schedules = require('./fubu-transportation-schedules');
var Subscriptions = require('./fubu-transportation-subscriptions');
var Tasks = require('./fubu-transportation-tasks');
var TransportsAndChannels = require('./fubu-transportation-transports');


FubuDiagnostics.addSection({
    title: 'FubuTransportation',
    description: 'Visualization of FubuTransportation services',
    key: 'fubu-transportation'
})
.add({
    title: 'Message Handlers',
    description: 'Registered handlers and their policies',
    key: 'handlers',
    component: Handlers
})
.add({
    title: 'Schedules',
    description: "Monitor the application's scheduled jobs",
    key: 'schedules',
    component: Schedules
})
.add({
    title: 'Subscriptions',
    description: 'Persisted subscriptions for this node',
    key: 'subscriptions',
    component: Subscriptions
})
.add({
    title: 'Tasks',
    description: 'What scheduled jobs are assigned are to each node',
    key: 'tasks',
    component: Tasks
})
.add({
    title: 'Transports and Channels',
    description: 'Details about the configured channels in this application',
    key: 'transports-and-channels',
    component: TransportsAndChannels
});
