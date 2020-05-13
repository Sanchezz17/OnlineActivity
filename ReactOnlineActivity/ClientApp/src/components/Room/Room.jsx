import React, {Component} from 'react';
import * as signalR from "@aspnet/signalr";
import Leaderboard from './Leaderboard/Leaderboard';
import Field from './Field/Field';
import Chat from './Chat/Chat';
import {RoomHubEvents} from './RoomConstants';
import styles from './room.module.css';


export default class Room extends Component {
    constructor(props) {
        super(props);
        this.roomId =  this.props.match.params.roomId;
    }

    componentWillUnmount() {
        this.hubConnection.invoke(RoomHubEvents.LEAVE_ROOM, this.roomId, this.props.user.name);
    }

    render() {
        const {user} = this.props;

        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl("/room")
            .build();
        this.hubConnection.start()
            .then(() => {
                console.log('Connection started!');
                this.hubConnection.invoke(RoomHubEvents.JOIN_ROOM, this.roomId, user.name);
            })
            .catch(err => console.log('Error while establishing connection :('));

        return (
            <section className={styles.room}>
                <Leaderboard roomId={this.roomId} user={user} hubConnection={this.hubConnection}/>
                <Field roomId={this.roomId} user={user} hubConnection={this.hubConnection}/>
                <Chat roomId={this.roomId} user={user} hubConnection={this.hubConnection}/>
            </section>
        );
    }
}