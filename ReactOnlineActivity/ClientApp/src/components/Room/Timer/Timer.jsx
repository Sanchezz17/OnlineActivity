import React, { Component } from 'react';
import { RoomHubEvents } from '../RoomConstants';
import styles from './timer.module.css';

export default class Timer extends Component {
    constructor(props) {
        super(props);
        this.state = {
            time: {
                minutesStr: '00',
                secondsStr: '00'
            },
            seconds: 0,
        };
        this.timer = 0;
    }

    async componentDidMount() {
        this.props.hubConnection.on(RoomHubEvents.TIME_LEFT, (seconds) => {
            this.setState({
                seconds: seconds + 1,
                time: this.secondsToTime(seconds + 1)
            });
        });
        this.props.hubConnection.on(RoomHubEvents.ROUND_INFO, (explainingPlayer) => {
            if (explainingPlayer !== null && this.timer === 0) {
                this.startTimer()
            }
        });
        await this.props.hubConnection.invoke(RoomHubEvents.REQUEST_TIME, this.props.roomId);
    }

    startTimer = () => {
        if (this.timer === 0 && this.state.seconds > 0) {
            this.timer = setInterval(this.countDown, 1000);
        }
    };

    secondsToTime = (totalSeconds) => {
        const divisorForMinutes = totalSeconds % (60 * 60);
        const minutes = Math.floor(divisorForMinutes / 60);
        const divisorForSeconds = divisorForMinutes % 60;
        const seconds = Math.ceil(divisorForSeconds);

        return {
            minutesStr: minutes.toString().padStart(2, '0'),
            secondsStr: seconds.toString().padStart(2, '0')
        };
    };

    countDown = async () => {
        if (this.state.seconds > 0) {
            let seconds = this.state.seconds - 1;
            this.setState({
                time: this.secondsToTime(seconds),
                seconds: seconds,
            });
        } else {
            clearInterval(this.timer);
            this.timer = 0;
            this.setState({
                time: this.secondsToTime(0),
                seconds: 0,
            });
            await this.props.hubConnection.invoke(RoomHubEvents.TIME_OVER, this.props.roomId);
        }
    };

    render() {
        return(
            <div className={styles.timer}>
                {this.state.time.minutesStr} : {this.state.time.secondsStr}
            </div>
        );
    }
}