import React, {Component, Fragment} from 'react';
import {NavItem, NavLink} from 'reactstrap';
import {Link} from 'react-router-dom';
import authService from '../AuthorizeService';
import {ApplicationPaths} from '../ApiAuthorizationConstants';
import styles from './loginMenu.module.css'

export class LoginMenu extends Component {
    constructor(props) {
        super(props);

        this.state = {
            isAuthenticated: false,
            userName: null,
            userPhotoUrl: null
        };
    }

    componentDidMount() {
        this._subscription = authService.subscribe(() => this.populateState());
        this.populateState();
    }

    componentWillUnmount() {
        authService.unsubscribe(this._subscription);
    }

    async populateState() {
        const [isAuthenticated, user] = await Promise.all([authService.isAuthenticated(), authService.getUser()]);
        this.setState({
            isAuthenticated,
            userName: user && user.name,
            userPhotoUrl: user && user.photoUrl
        });
    }

    render() {
        const { isAuthenticated, userName } = this.state;
        if (!isAuthenticated) {
            const registerPath = `${ApplicationPaths.Register}`;
            const loginPath = `${ApplicationPaths.Login}`;
            return this.anonymousView(registerPath, loginPath);
        } else {
            const profilePath = `${ApplicationPaths.Profile}`;
            const logoutPath = {pathname: `${ApplicationPaths.LogOut}`, state: {local: true}};
            return this.authenticatedView(userName, profilePath, logoutPath);
        }
    }

    renderUserPhoto() {
        const { userPhotoUrl, userName } = this.state;
        if (userPhotoUrl) {
            return (
                <NavItem>
                    <img src={userPhotoUrl} alt={userName} className={styles.image}/>
                </NavItem>
            )
        }
    }

    authenticatedView(userName, profilePath, logoutPath) {
        return (<Fragment>
            <NavItem>
                <NavLink tag={Link} className="text-dark" to={profilePath}>Привет, {userName}</NavLink>
            </NavItem>
            {this.renderUserPhoto()}
            <NavItem>
                <NavLink tag={Link} className="text-dark" to={logoutPath}>Выйти</NavLink>
            </NavItem>
        </Fragment>);
    }

    anonymousView(registerPath, loginPath) {
        return (<Fragment>
            <NavItem>
                <NavLink tag={Link} className="text-dark" to={registerPath}>Зарегистрироваться</NavLink>
            </NavItem>
            <NavItem>
                <NavLink tag={Link} className="text-dark" to={loginPath}>Войти</NavLink>
            </NavItem>
        </Fragment>);
    }
}
