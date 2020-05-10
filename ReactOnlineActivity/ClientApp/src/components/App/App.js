import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from '../Layout/Layout';
import { Home } from '../Home/Home';
import AuthorizeRoute from '../api-authorization/AuthorizeRoute';
import ApiAuthorizationRoutes from '../api-authorization/ApiAuthorizationRoutes';
import { ApplicationPaths } from '../api-authorization/ApiAuthorizationConstants';

import './app.module.css'

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
        <AuthorizeRoute exact path='/' component={Home} />
        <Route path={ApplicationPaths.ApiAuthorizationPrefix} component={ApiAuthorizationRoutes} />
      </Layout>
    );
  }
}
