import React, {Component} from 'react';
import Leaderboard from './Leaderboard/Leaderboard';
import Field from './Field/Field';
import Chat from './Chat/Chat';
import styles from './room.module.css';


export default class Room extends Component {
    render() {
        const roomId = this.props.match.params.roomId;
        const { user } = this.props;
        
        return (
            <section className={styles.room}>
                <Leaderboard roomId={roomId} user={user}/>
                <Field roomId={roomId} user={user}/>
                <Chat roomId={roomId} user={user}/>
            </section>
        );
    }
}