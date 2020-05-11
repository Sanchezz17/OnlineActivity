import React, {Component} from 'react';
import Board from './Leaderboard/Leaderboard';
import Field from './Field/Field';
import Chat from './Chat/Chat';
import styles from './room.module.css';


export default class Room extends Component {
    state = {
        user: null
    }

    render() {
        const { roomId, user } = this.props;

        return (
            <section className={styles.room}>
                <Board roomId={roomId} user={user}/>
                <Field roomId={roomId} user={user}/>
                <Chat roomId={roomId} user={user}/>
            </section>
        );
    }
}