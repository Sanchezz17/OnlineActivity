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
        this.roomId = this.props.match.params.roomId;
        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl("/room")
            .build();

        this.state = {
            loading: true
        }
    }

    async componentDidMount() {
        await this.hubConnection.start()
        this.setState({loading: false})
        const {user} = this.props;
        this.hubConnection.invoke(RoomHubEvents.JOIN_ROOM, this.roomId, user.name, user.photoUrl);

    }

    componentWillUnmount() {
        const { user } = this.props;
        this.hubConnection.invoke(RoomHubEvents.LEAVE_ROOM, this.roomId, user.name);
        fetch(`/api/leave?roomId=${this.props.roomId}&playerName=${user.name}`)
    }

    render() {
        const {user} = this.props;

        return (
            this.state.loading
                ? <div className={styles.loading}>
                    <p>Загрузка игры...</p>
                </div>
                : <section className={styles.room}>
                    <Leaderboard roomId={this.roomId} user={user} hubConnection={this.hubConnection}/>
                    <Field roomId={this.roomId} user={user} hubConnection={this.hubConnection}/>
                    <Chat roomId={this.roomId} user={user} hubConnection={this.hubConnection}/>
                </section>
        );
    }
}