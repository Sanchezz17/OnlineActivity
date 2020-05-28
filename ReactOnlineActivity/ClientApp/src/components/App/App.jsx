import React, { Component } from 'react';
import { Route } from 'react-router';
import { Switch } from 'react-router-dom';
import ApiAuthorizationRoutes from '../api-authorization/ApiAuthorizationRoutes';
import AuthorizeRoute from '../api-authorization/AuthorizeRoute';
import CreateRoom from '../CreateRoom/CreateRoom';
import NotFound from '../NotFound/NotFound';
import Room from '../Room/Room';
import { ApplicationPaths } from '../api-authorization/ApiAuthorizationConstants';
import { Home } from '../Home/Home';
import { Layout } from '../Layout/Layout';
import './app.module.css';

export default class App extends Component {
    static displayName = App.name;

    render() {
        return (
            <Layout>
                <Switch>
                    <AuthorizeRoute exact path='/' component={Home}/>
                    <AuthorizeRoute path='/rooms/:roomId' component={Room}/>
                    <AuthorizeRoute path='/create' component={CreateRoom}/>
                    <Route path={ApplicationPaths.ApiAuthorizationPrefix} component={ApiAuthorizationRoutes}/>
                    <Route path="*" component={NotFound}/>
                </Switch>
            </Layout>
        );
    }
}
