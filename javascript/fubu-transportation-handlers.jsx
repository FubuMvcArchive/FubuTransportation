/** @jsx React.DOM */

var React = FubuDiagnostics.React;

var HandlerItem = React.createClass({
  render: function(){
    var methods = this.props.methods.map(function(method) {
      return (<li>{method}</li>);
    });
    return (
      <tr>
        <td class="message">
          <h4>{this.props.name}</h4>
          <p>{this.props.namespace}</p>
        </td>
        <td>
          TODO: HtmlTags from the Description for the ExceptionHandlerNode needs to render here
        </td>
        <td>
          TODO: HtmlTags for the Descriptions of the other nodes need to render here
        </td>
        <td>
          <ul>{methods}</ul>
        </td>
      </tr>
    );
  }
});

var Handlers = React.createClass({
  render: function(){
    var items = this.state.handlers.map(function(handler, i){
      return HandlerItem(handler);
    });

    return (
      <div>
        <h1>Message Handler Chains</h1>
        <table class="table handlers">
          <thead>
            <tr>
              <th>Message</th>
              <th>Exception Policies</th>
              <th>Other</th>
              <th>Handlers</th>
            </tr>
          </thead>
          <tbody>
            {items}
          </tbody>
        </table>
      </div>
    );
  }
})