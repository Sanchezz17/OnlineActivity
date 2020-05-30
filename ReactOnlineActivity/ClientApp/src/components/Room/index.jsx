import React, { Component } from 'react';
import { CopyToClipboard } from 'react-copy-to-clipboard';
import { HubConnectionState } from '@aspnet/signalr';
import * as signalR from '@aspnet/signalr';
import authorizeFetch from '../../utils/authorizeFetch';
import Chat from './Chat';
import Field from './Field';
import Leaderboard from './Leaderboard';
import { RoomHubEvents } from './RoomConstants';
import styles from './room.module.css';

export default class Room extends Component {
    constructor(props) {
        super(props);
        this.roomId = this.props.match.params.roomId;
        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl('/room')
            .build();

        this.state = {
            loading: true
        };
    }

    async componentDidMount() {
        await this.hubConnection.start();
        const { user } = this.props;
        const joinRoomDto = await authorizeFetch(`/api/rooms/${this.roomId}/join?userName=${user.name}`);
        await this.hubConnection.invoke(RoomHubEvents.JOIN_ROOM, this.roomId, user.name, joinRoomDto.alreadyInRoom);
        if (!joinRoomDto.alreadyInRoom) {
            await this.hubConnection.invoke(RoomHubEvents.NEW_PLAYER, this.roomId, joinRoomDto.player);
        }
        const roomResponse = await fetch(`/api/rooms/${this.roomId}`);
        this.room = await roomResponse.json();
        this.setState({ loading: false });

        window.addEventListener('beforeunload', this.beforeUnload);
    }
    
    beforeUnload = async () => {
        const { user } = this.props;
        if (this.hubConnection.state === HubConnectionState.Connected)
            await this.hubConnection.invoke(RoomHubEvents.LEAVE_ROOM, this.roomId, user.name);
    }

    async componentWillUnmount() {
        await this.beforeUnload();
    }

    render() {
        const { user } = this.props;

        return (
            this.state.loading
                ? <div className={styles.loading}>
                    <p>Загрузка игры...</p>
                </div>
                :  <div>
                    <div className={styles.nameContainer}>
                        <span className={styles.name}>{this.room.settings.name}</span>
                        <CopyToClipboard text={`${window.location.host}/rooms/${this.roomId}`}>
                            <span className={styles.copy}>Скопировать ссылку на комнату</span>
                        </CopyToClipboard>
                    </div>
                    <p>{this.room.settings.description}</p>
                    <section className={styles.room}>
                        <Leaderboard roomId={this.roomId} user={user} hubConnection={this.hubConnection}/>
                        <Field roomId={this.roomId} user={user} hubConnection={this.hubConnection}/>
                        <Chat roomId={this.roomId} user={user} hubConnection={this.hubConnection}/>
                    </section>
                </div>
        );
    }
}