import React, { Component } from 'react';
import authorizeFetch from '../../utils/authorizeFetch';
import styles from './leaderboard.module.css';

export default class Leaderboard extends Component {
    constructor(props) {
        super(props);
    }

    componentDidMount() {

    }

    render() {
        const { user } = this.props;
        
        return (
            <>
                <h1>Список лидеров</h1>
            </>
        );
    }
}