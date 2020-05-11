import React, {Component} from 'react';
import styles from './leaderboard.module.css';

export default class Board extends Component {
    state = {
        users: [],
        loadingUsers: false
    }

    componentDidMount() {
        this.addUser()
        setInterval(this.fetchUsers, 500);
    }

    fetchUsers = () => {
        fetch(`/api/users/${this.props.roomId}`)
            .then(response => response.json())
            .then(users => this.setState({
                ...this.state,
                loadingUsers: false,
                users
            }))
    };

    addUser = () => {
        fetch(`/api/users/${this.props.roomId}`, {
            body: JSON.stringify(this.props.user),
            headers: {'Content-Type': 'application/json'},
            method: 'POST'
        }).then(this.fetchUsers)
    };

    render() {
        if (this.state.loadingUsers) {
            return <p>Loading...</p>
        }

        return (
            <section className={styles.board}>
                {this.state.users.map(user => <p>{user.username}</p>)}
            </section>
        );
    }
}
